namespace ChatApp.ChatService.Application.DTOs
{
    public record MessageDto(
        Guid Id,
        Guid RoomId,
        Guid SenderId,
        string SenderUsername,
        string? SenderAvatarUrl,
        string Content,
        string MessageType,
        DateTime SentAt
    );

    public record RoomDto(
        Guid Id,
        string Name,
        string? Description,
        Guid CreatedByUserId,
        DateTime CreatedAt
    );

    public record ChatUserDto(
        Guid Id,
        string Username,
        string? AvatarUrl
    );
}