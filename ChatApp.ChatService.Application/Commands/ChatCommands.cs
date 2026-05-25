using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Application.Commands
{
    public class ChatCommands
    {
        public record SendMessageCommand(Guid RoomId, Guid SenderId, string Content);
        public record CreateRoomCommand(string Name, string? Description, Guid CreatedByUserId);
        public record DeleteMessageCommand(Guid MessageId, Guid RequestedByUserId);
    }
}
