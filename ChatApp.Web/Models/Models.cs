namespace ChatApp.Web.Models
{
    public record LoginRequest(string Email, string Password);

    public record RegisterRequest(string Email, string Username, string Password);

    public record TokenResponse(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string TokenType
    );

    public record UserInfo(
        Guid Id,
        string Username,
        string? AvatarUrl,
        DateTime LastSeenAt
    );

    public record RoomDto(
        Guid Id,
        string Name,
        string? Description,
        Guid CreatedByUserId,
        DateTime CreatedAt
    );

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

    public record CreateRoomRequest(string Name, string? Description);

}
