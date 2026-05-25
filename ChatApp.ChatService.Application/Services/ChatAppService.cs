using ChatApp.AuthService.Domain.Exceptions;
using ChatApp.ChatService.Application.DTOs;
using ChatApp.ChatService.Application.Interfaces;
using ChatApp.ChatService.Application.Queries;
using ChatApp.ChatService.Domain.Entities;
using ChatApp.ChatService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChatApp.ChatService.Application.Commands.ChatCommands;

namespace ChatApp.ChatService.Application.Services
{
    public class ChatAppService : IChatService
    {
        private readonly IMessageRepository _messageRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IChatUserRepository _userRepo;
        private readonly ICacheService _cache;

        private const string UserCacheKeyPrefix = "chat:user:";
        private static readonly TimeSpan UserCacheLifetime = TimeSpan.FromMinutes(5);

        public ChatAppService(IMessageRepository messageRepo, IRoomRepository roomRepo, IChatUserRepository userRepo, ICacheService cache)
        {
            _messageRepo = messageRepo;
            _roomRepo = roomRepo;
            _userRepo = userRepo;
            _cache = cache;
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageCommand command)
        {
            var room = await _roomRepo.GetByIdAsync(command.RoomId)
            ?? throw new DomainException($"Room {command.RoomId} not found");

            var message = Message.Create(command.RoomId, command.SenderId, command.Content);
            await _messageRepo.AddAsync(message);

            var sender = await GetCachedUserAsync(command.SenderId);

            return new MessageDto(
               message.Id,
               message.RoomId,
               message.SenderId,
               sender?.Username ?? "Unknown",
               sender?.AvatarUrl,
               message.Content,
               message.SentAt
           );
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomCommand command)
        {
            var room = Room.Create(command.Name, command.Description, command.CreatedByUserId);
            await _roomRepo.AddAsync(room);

            return new RoomDto(room.Id, room.Name, room.Description, room.CreatedByUserId, room.CreatedAt);
        }

        public async Task DeleteMessageAsync(DeleteMessageCommand command)
        {
            var message = await _messageRepo.GetByIdAsync(command.MessageId)
                ?? throw new DomainException("Message not found");

            if (message.SenderId != command.RequestedByUserId)
                throw new DomainException("You can only delete your own messages");

            message.SoftDelete();
            await _messageRepo.UpdateAsync(message);
        }

        public async Task<IEnumerable<MessageDto>> GetRoomHistoryAsync(GetRoomHistoryQuery query)
        {
            var messages = await _messageRepo.GetByRoomAsync(query.RoomId, query.Page, query.PageSize);

            // ดึง user info ทีละคน (พร้อม cache เพื่อลด DB hits)
            var result = new List<MessageDto>();
            foreach (var msg in messages)
            {
                var sender = await GetCachedUserAsync(msg.SenderId);
                result.Add(new MessageDto(
                    msg.Id, msg.RoomId, msg.SenderId,
                    sender?.Username ?? "Unknown",
                    sender?.AvatarUrl,
                    msg.Content, msg.SentAt));
            }

            // ส่งกลับเรียงจากเก่า → ใหม่ (เพราะ DB query เรียง DESC)
            return result.OrderBy(m => m.SentAt);
        }

        public async Task<IEnumerable<RoomDto>> GetAllRoomAsync()
        {
            var rooms = await _roomRepo.GetAllAsync();
            return rooms.Select(r =>
                new RoomDto(r.Id, r.Name, r.Description, r.CreatedByUserId, r.CreatedAt));
        }

        public async Task UpsertChatUserAsync(Guid userId, string username)
        {
            var existing = await _userRepo.GetByIdAsync(userId);
            var user = existing ?? new ChatUser { Id = userId, AvatarUrl = null };
            user.Username = username;
            user.LastSeenAt = DateTime.UtcNow;

            await _userRepo.UpsertAsync(user);

            // invalidate cache เพื่อให้ครั้งต่อไปดึงค่าใหม่
            await _cache.RemoveAsync(UserCacheKeyPrefix + userId);
        }

        private async Task<ChatUser?> GetCachedUserAsync(Guid userId)
        {
            var key = UserCacheKeyPrefix + userId;
            var cached = await _cache.GetAsync<ChatUser>(key);
            if (cached is not null) return cached;

            var user = await _userRepo.GetByIdAsync(userId);
            if (user is not null)
                await _cache.SetAsync(key, user, UserCacheLifetime);

            return user;
        }

    }
}
