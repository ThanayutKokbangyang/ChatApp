using Blazored.LocalStorage;
using ChatApp.Web;
using ChatApp.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var baseAddress = builder.HostEnvironment.BaseAddress;

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddHttpClient("anonymous", client =>
{
    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddHttpClient("authorized", client =>
{
    client.BaseAddress = new Uri(baseAddress);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("anonymous"));

builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<ChatApiService>();

await builder.Build().RunAsync();