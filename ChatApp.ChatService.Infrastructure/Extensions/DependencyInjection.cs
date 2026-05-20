using ChatApp.ChatService.Domain.Interfaces;
using ChatApp.ChatService.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Infrastructure.Extensions
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
