using ChatApp.ChatService.Application.DTOs;
using ChatApp.ChatService.Application.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChatApp.ChatService.Application.Commands.ChatCommands;

namespace ChatApp.ChatService.Application.Interfaces
{
    public interface IChatService
    {
        Task<MessageDto> SendMessageAsync(SendMessageCommand command);
        Task<RoomDto> CreateRoomAsync(CreateRoomCommand command);
        Task DeleteMessageAsync(DeleteMessageCommand command);
        Task<IEnumerable<MessageDto>> GetRoomHistoryAsync(GetRoomHistoryQuery query);
        Task<IEnumerable<RoomDto>> GetAllRoomAsync();
        Task UpsertChatUserAsync(Guid userId, string username);

    }
}
