using ChatApp.ChatService.Application.Interfaces;
using ChatApp.ChatService.Application.Queries;
using ChatApp.ChatService.Domain.Exceptions;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static ChatApp.ChatService.Application.Commands.ChatCommands;

namespace ChatApp.ChatService.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task onConnectedAsync()
        {
            var userId = GetUserId();
            var username = GetUsername();

            await _chatService.UpsertChatUserAsync(userId, username);
            await base.OnConnectedAsync();

        }    

        public async Task JoinRoom(string roomId)
        {
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
                var command = new SendMessageCommand(Guid.Parse(roomId), GetUserId(), content);

                var message = await _chatService.SendMessageAsync(command);

                await Clients.Group(roomId).SendAsync("ReceiveMessage", message);
            }
            catch (DomainException ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task SendTyping(string roomId, bool isTyping)
        {
            await Clients.OthersInGroup(roomId).SendAsync("UserTyping", new
            {
                Username = GetUsername(),
                IsTyping = isTyping
            });
        }

        private Guid GetUserId()
        {
            var sub = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? throw new HubException("User identity not found");
            return Guid.Parse(sub);
        }
        private string GetUsername()
        {
            return Context.User?.FindFirstValue(ClaimTypes.Name) ?? Context.User?.Identity?.Name ?? "Unknown";
        }
    }
}
