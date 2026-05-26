using ChatApp.Web.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

namespace ChatApp.Web.Services
{
    public class ChatApiService : IAsyncDisposable
    {
        private readonly HttpClient _http;
        private readonly AuthApiService _auth;
        private HubConnection? _hubConnection;

        public event Action<MessageDto>? MessageReceived;
        public event Action<IEnumerable<MessageDto>>? HistoryReceived;
        public event Action<string>? UserJoined;
        public event Action<string>? UserLeft;
        public event Action<string>? ErrorOccurred;
        public event Action<string, bool>? UserTyping;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public ChatApiService(IHttpClientFactory httpClientFactory, AuthApiService auth)
        {
            _http = httpClientFactory.CreateClient("authorized");
            _auth = auth;
        }

        public async Task StartAsync(string hubUrl)
        {
            if (_hubConnection?.State == HubConnectionState.Connected) return;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = async () => await _auth.GetAccessTokenAsync();
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<MessageDto>("ReceiveMessage", msg =>
                MessageReceived?.Invoke(msg));

            _hubConnection.On<IEnumerable<MessageDto>>("ReceiveHistory", history =>
                HistoryReceived?.Invoke(history));

            _hubConnection.On<string>("UserJoined", username =>
                UserJoined?.Invoke(username));

            _hubConnection.On<string>("UserLeft", username =>
                UserLeft?.Invoke(username));

            _hubConnection.On<string>("ErrorOccurred", error =>
                ErrorOccurred?.Invoke(error));

            _hubConnection.On<string, bool>("UserTyping", (username, isTyping) =>
                UserTyping?.Invoke(username, isTyping));

            await _hubConnection.StartAsync();
        }

        public async Task JoinRoomAsync(Guid roomId)
        {
            if (_hubConnection is null) return;
            await _hubConnection.InvokeAsync("JoinRoom", roomId.ToString());
        }

        public async Task LeaveRoomAsync(Guid roomId)
        {
            if (_hubConnection is null) return;
            await _hubConnection.InvokeAsync("LeaveRoom", roomId.ToString());
        }

        public async Task SendMessageAsync(Guid roomId, string content)
        {
            if (_hubConnection is null) return;
            await _hubConnection.InvokeAsync("SendMessage", roomId.ToString(), content);
        }

        public async Task SendTypingAsync(Guid roomId, bool isTyping)
        {
            if (_hubConnection is null) return;

            await _hubConnection.InvokeAsync(
                "SendTyping",
                roomId.ToString(),
                isTyping);
        }

        public async Task<IEnumerable<RoomDto>?> GetRoomsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<RoomDto>>("/chat/api/rooms");

        public async Task<RoomDto?> CreateRoomAsync(string name, string? description)
        {
            var response = await _http.PostAsJsonAsync(
                "/chat/api/rooms",
                new CreateRoomRequest(name, description));

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<RoomDto>()
                : null;
        }

        public async Task<UserInfo?> GetMyProfileAsync()
            => await _http.GetFromJsonAsync<UserInfo>("/chat/api/profile/me");

        public event Action<string>? AvatarUpdated;

        public async Task<string?> UploadAvatarAsync(Stream fileStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", fileName);

            var response = await _http.PostAsync("/chat/api/profile/avatar", content);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

            if (result is not null && result.TryGetValue("avatarUrl", out var avatarUrl))
            {
                AvatarUpdated?.Invoke(avatarUrl);
                return avatarUrl;
            }

            return null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
                await _hubConnection.DisposeAsync();
        }
    }
}