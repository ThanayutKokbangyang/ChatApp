using ChatApp.AuthService.Application.Commands;
using ChatApp.AuthService.Application.Interfaces;
using ChatApp.AuthService.Domain.Exceptions;
using System.Security.Claims;

namespace ChatApp.Auth.Service.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/auth").WithTags("Authentication");

            group.MapPost("/register", RegisterAsync);
            group.MapPost("/login", LoginAsync);
            group.MapPost("/refresh", RefreshAsync);
            group.MapPost("/revoke", RevokeAsync).RequireAuthorization();
            group.MapGet("/me", GetCurrentUserAsync).RequireAuthorization();
        }

        private static async Task<IResult> RegisterAsync(RegisterCommand command, IAuthService authService)
        {
            try
            {
                var user = await authService.RegisterAsync(command);
                return Results.Created($"/api/users/{user.Id}", user);
            }
            catch (DomainException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        private static async Task<IResult> LoginAsync(LoginCommand command, IAuthService authService)
        {
            try
            {
                var tokens = await authService.LoginAsync(command);
                return Results.Ok(tokens);
            }
            catch (DomainException ex)
            {
                return Results.Unauthorized();
            }
        }

        private static async Task<IResult> RefreshAsync(RefreshTokenCommand command, IAuthService authService)
        {
            try
            {
                var tokens = await authService.RefreshTokenAsync(command);
                return Results.Ok(tokens);
            }
            catch (DomainException)
            {
                return Results.Unauthorized();
            }
        }

        private static async Task<IResult> RevokeAsync(RevokeTokenCommand command, IAuthService authService)
        {
            await authService.RevokeTokenAsync(command);
            return Results.NoContent();
        }

        private static IResult GetCurrentUserAsync(ClaimsPrincipal user)
        {
            return Results.Ok(new
            {
                Id = user.FindFirstValue("sub"),
                Email = user.FindFirstValue("email"),
                Username = user.Identity?.Name,
                Role = user.FindFirstValue(ClaimTypes.Role)
            });
        }
    }
}
