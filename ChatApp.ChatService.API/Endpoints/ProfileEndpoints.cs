using ChatApp.ChatService.Application.Interfaces;
using ChatApp.ChatService.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ChatApp.ChatService.API.Endpoints
{
    public static class ProfileEndpoints
    {
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
        private const string AvatarFolder = "uploads/avatars";

        public static void MapProfileEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/profile").WithTags("Profile").RequireAuthorization();

            group.MapPost("/avatar", UploadAvatarAsync).DisableAntiforgery();

            group.MapGet("/me", GetMyProfileAsync);
        }

        private static async Task<IResult> UploadAvatarAsync(IFormFile file, ClaimsPrincipal user, IChatUserRepository userRepo, IFileStorageService fileStorage)
        {
            if (file.Length == 0)
                return Results.BadRequest(new { error = "File is empty" });

            if (file.Length > MaxFileSizeBytes)
                return Results.BadRequest(new { error = "File size exceeds 5 MB" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return Results.BadRequest(new
                {
                    error = $"Allowed extensions: {string.Join(", ", AllowedExtensions)}"
                });

            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var fileName = $"{userId}{ext}";

            await using var stream = file.OpenReadStream();
            var avatarUrl = await fileStorage.SaveAsync(stream, fileName, AvatarFolder);

            await userRepo.UpdateAvatarAsync(userId, avatarUrl);
            return Results.Ok(new { avatarUrl });
        }

        private static async Task<IResult> GetMyProfileAsync(ClaimsPrincipal user, IChatUserRepository userRepo) {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var chatUser = await userRepo.GetByIdAsync(userId);

            if (chatUser is null)
                return Results.NotFound();

            return Results.Ok(new
            {
                chatUser.Id,
                chatUser.Username,
                chatUser.AvatarUrl,
                chatUser.LastSeenAt
            });

        }
    }
}
