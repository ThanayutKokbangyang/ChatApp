namespace ChatApp.ChatService.Application.Commands
{
    public class ChatCommands
    {
        public record SendMessageCommand(
            Guid RoomId,
            Guid SenderId,
            string Content,
            string MessageType = "text");

        public record CreateRoomCommand(string Name, string? Description, Guid CreatedByUserId);
        public record DeleteMessageCommand(Guid MessageId, Guid RequestedByUserId);
    }
}