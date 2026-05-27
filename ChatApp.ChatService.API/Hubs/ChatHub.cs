using ChatApp.ChatService.Application.Interfaces;
using ChatApp.ChatService.Application.Queries;
using ChatApp.ChatService.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static ChatApp.ChatService.Application.Commands.ChatCommands;

namespace ChatApp.ChatService.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            var username = GetUsername();

            await _chatService.UpsertChatUserAsync(userId, username);

            await base.OnConnectedAsync();
        }

        public async Task JoinRoom(string roomId)
        {
            var userId = GetUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            var history = await _chatService.GetRoomHistoryAsync(
                new GetRoomHistoryQuery(Guid.Parse(roomId), 1, 50));

            await Clients.Caller.SendAsync("ReceiveHistory", history);
            await Clients.OthersInGroup(roomId).SendAsync("UserJoined", GetUsername());
        }

        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.OthersInGroup(roomId).SendAsync("UserLeft", GetUsername());
        }

        public async Task SendMessage(string roomId, string content)
        {
            try
            {
                var command = new SendMessageCommand(
                    Guid.Parse(roomId),
                    GetUserId(),
                    content,
                    "text");

                var message = await _chatService.SendMessageAsync(command);

                await Clients.Group(roomId).SendAsync("ReceiveMessage", message);
            }
            catch (DomainException ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
            }
        }

        public async Task SendImageMessage(string roomId, string imageUrl)
        {
            try
            {
                var command = new SendMessageCommand(
                    Guid.Parse(roomId),
                    GetUserId(),
                    imageUrl,
                    "image");

                var message = await _chatService.SendMessageAsync(command);

                await Clients.Group(roomId).SendAsync("ReceiveMessage", message);
            }
            catch (DomainException ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
            }
        }

        public async Task SendTyping(string roomId, bool isTyping)
        {
            await Clients.OthersInGroup(roomId)
                .SendAsync("UserTyping", GetUsername(), isTyping);
        }

        private Guid GetUserId()
        {
            var userId =
                Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                Context.User?.FindFirstValue("sub") ??
                Context.User?.FindFirstValue("nameid") ??
                Context.User?.FindFirstValue("userId");

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("User identity not found");
            }

            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                throw new HubException($"Invalid user id claim: {userId}");
            }

            return parsedUserId;
        }

        private string GetUsername()
        {
            return
                Context.User?.FindFirstValue(ClaimTypes.Name) ??
                Context.User?.FindFirstValue(JwtRegisteredClaimNames.Name) ??
                Context.User?.FindFirstValue("username") ??
                Context.User?.Identity?.Name ??
                "Unknown";
        }

        public async Task SendStickerMessage(string roomId, string sticker)
        {
            try
            {
                var command = new SendMessageCommand(
                    Guid.Parse(roomId),
                    GetUserId(),
                    sticker,
                    "sticker");

                var message = await _chatService.SendMessageAsync(command);

                await Clients.Group(roomId).SendAsync("ReceiveMessage", message);
            }
            catch (DomainException ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
            }
        }
    }
}