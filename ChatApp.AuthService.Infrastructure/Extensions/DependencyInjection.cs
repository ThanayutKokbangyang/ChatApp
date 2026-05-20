using ChatApp.AuthService.Application.Interfaces;
using ChatApp.AuthService.Application.Services;
using ChatApp.AuthService.Domain.Interfaces;
using ChatApp.AuthService.Infrastructure.Data;
using ChatApp.AuthService.Infrastructure.Repositories;
using ChatApp.AuthService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using AuthServiceImpl = ChatApp.AuthService.Application.Services.AuthService;

namespace ChatApp.AuthService.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

            services.AddScoped<IAuthService, AuthServiceImpl>();

            return services;
        }
    }
}
