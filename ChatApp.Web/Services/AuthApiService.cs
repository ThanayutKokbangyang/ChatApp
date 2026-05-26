using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using ChatApp.Web.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace ChatApp.Web.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public const string AccessTokenKey = "access_token";
        public const string RefreshTokenKey = "refresh_token";

        public AuthApiService(
            IHttpClientFactory httpClientFactory,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authStateProvider)
        {
            _http = httpClientFactory.CreateClient("anonymous");
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var response = await _http.PostAsJsonAsync(
                "/auth/api/auth/login",
                new LoginRequest(email, password));

            if (!response.IsSuccessStatusCode) return false;

            var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokens is null) return false;

            await _localStorage.SetItemAsync(AccessTokenKey, tokens.AccessToken);
            await _localStorage.SetItemAsync(RefreshTokenKey, tokens.RefreshToken);

            ((CustomAuthStateProvider)_authStateProvider)
                .NotifyUserAuthentication(tokens.AccessToken);

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            return true;
        }

        public async Task<bool> RegisterAsync(string email, string username, string password)
        {
            var response = await _http.PostAsJsonAsync(
                "/auth/api/auth/register",
                new RegisterRequest(email, username, password));

            return response.IsSuccessStatusCode;
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(AccessTokenKey);
            await _localStorage.RemoveItemAsync(RefreshTokenKey);

            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<string?> GetAccessTokenAsync()
            => await _localStorage.GetItemAsync<string>(AccessTokenKey);
    }
}