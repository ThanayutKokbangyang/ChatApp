using ChatApp.ChatService.Application.Interfaces;
using ChatApp.ChatService.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
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
            var group = app.MapGroup("/api/profile")
                .WithTags("Profile")
                .RequireAuthorization();

            group.MapPost("/avatar", UploadAvatarAsync)
                .DisableAntiforgery();

            group.MapGet("/me", GetMyProfileAsync);
        }

        private static async Task<IResult> UploadAvatarAsync(
            IFormFile file,
            ClaimsPrincipal user,
            IChatUserRepository userRepo,
            IFileStorageService fileStorage)
        {
            try
            {
                if (file.Length == 0)
                    return Results.BadRequest(new { error = "File is empty" });

                if (file.Length > MaxFileSizeBytes)
                    return Results.BadRequest(new { error = "File size exceeds 5 MB" });

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    return Results.BadRequest(new
                    {
                        error = $"Allowed extensions: {string.Join(", ", AllowedExtensions)}"
                    });
                }

                var userId = GetUserId(user);
                var fileName = $"{userId}{ext}";

                await using var stream = file.OpenReadStream();
                var avatarUrl = await fileStorage.SaveAsync(stream, fileName, AvatarFolder);

                await userRepo.UpdateAvatarAsync(userId, avatarUrl);

                return Results.Ok(new { avatarUrl });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        private static async Task<IResult> GetMyProfileAsync(
            ClaimsPrincipal user,
            IChatUserRepository userRepo)
        {
            try
            {
                var userId = GetUserId(user);
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
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var userId =
                user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user.FindFirstValue("sub") ??
                user.FindFirstValue("nameid") ??
                user.FindFirstValue("userId") ??
                user.FindFirstValue("uid");

            if (string.IsNullOrWhiteSpace(userId))
            {
                var claims = user.Claims.Select(c => $"{c.Type}={c.Value}");
                throw new InvalidOperationException(
                    "User identity not found. Claims: " + string.Join(" | ", claims));
            }

            if (!Guid.TryParse(userId, out var parsedUserId))
                throw new InvalidOperationException($"Invalid user id claim: {userId}");

            return parsedUserId;
        }
    }
}