using ChatApp.ChatService.Application.Interfaces;
using ChatApp.ChatService.Application.Queries;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static ChatApp.ChatService.Application.Commands.ChatCommands;

namespace ChatApp.ChatService.API.Endpoints
{
    public static class RoomEndpoints
    {
        public static void MapRoomEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/rooms")
                .WithTags("Rooms")
                .RequireAuthorization();

            group.MapGet("/", GetAllRoomsAsync);
            group.MapPost("/", CreateRoomAsync);
            group.MapGet("/{roomId:guid}/messages", GetRoomHistoryAsync);
        }

        private static async Task<IResult> GetAllRoomsAsync(IChatService chatService)
        {
            var rooms = await chatService.GetAllRoomAsync();
            return Results.Ok(rooms);
        }

        private static async Task<IResult> CreateRoomAsync(CreateRoomRequest request, ClaimsPrincipal user, IChatService chatService)
        {
            try
            {
                var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
                var command = new CreateRoomCommand(request.Name, request.Description, userId);
                var room = await chatService.CreateRoomAsync(command);
                return Results.Created($"/api/rooms/{room.Id}", room);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        private static async Task<IResult> GetRoomHistoryAsync(Guid roomId, IChatService chatService, int page = 1, int pageSize = 50)
        {
            var messages = await chatService.GetRoomHistoryAsync(
            new GetRoomHistoryQuery(roomId, page, pageSize));
            return Results.Ok(messages);
        }

        public record CreateRoomRequest(string Name, string? Description);

    }
}
