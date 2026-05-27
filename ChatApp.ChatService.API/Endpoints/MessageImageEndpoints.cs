using ChatApp.ChatService.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ChatApp.ChatService.API.Endpoints
{
    public static class MessageImageEndpoints
    {
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;
        private const string ImageFolder = "uploads/chat-images";

        public static void MapMessageImageEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/messages")
                .WithTags("Messages")
                .RequireAuthorization();

            group.MapPost("/image", UploadImageAsync)
                .DisableAntiforgery();
        }

        private static async Task<IResult> UploadImageAsync(
            IFormFile file,
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

                var fileName = $"{Guid.NewGuid()}{ext}";

                await using var stream = file.OpenReadStream();
                var imageUrl = await fileStorage.SaveAsync(stream, fileName, ImageFolder);

                return Results.Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }
    }
}