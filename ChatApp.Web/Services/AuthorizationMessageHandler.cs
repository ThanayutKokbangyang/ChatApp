using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace ChatApp.Web.Services
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public AuthorizationMessageHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage,
            CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>(AuthApiService.AccessTokenKey);

            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(requestMessage, cancellationToken);
        }
    }
}