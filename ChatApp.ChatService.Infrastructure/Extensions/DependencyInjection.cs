using ChatApp.ChatService.Application.Interfaces;
using ChatApp.ChatService.Application.Services;
using ChatApp.ChatService.Domain.Interfaces;
using ChatApp.ChatService.Infrastructure.Data;
using ChatApp.ChatService.Infrastructure.Repositories;
using ChatApp.ChatService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.ChatService.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddChatInfrastructure(this IServiceCollection services)
        {
            // Factory Pattern
            services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

            // Repository Pattern (scoped per request)
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IChatUserRepository, ChatUserRepository>();

            // Strategy Pattern: cache + storage
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();

            // Facade
            services.AddScoped<IChatService, ChatAppService>();

            return services;
        }
    }
}
