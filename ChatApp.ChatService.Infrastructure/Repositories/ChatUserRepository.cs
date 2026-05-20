using ChatApp.ChatService.Domain.Entities;
using ChatApp.ChatService.Domain.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Infrastructure.Repositories
{
    public class ChatUserRepository : IChatUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ChatUserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<ChatUser?> GetByIdAsync(Guid id)
        {
            const string sql = "SELECT Id, Username, AvatarUrl, LastSeenAt FROM ChatUsers WHERE Id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ChatUser>(sql, new { Id = id });
        }

        public async Task UpsertAsync(ChatUser user)
        {
            const string sql = """
            MERGE ChatUsers AS target
            USING (SELECT @Id AS Id) AS source ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET Username = @Username, LastSeenAt = @LastSeenAt
            WHEN NOT MATCHED THEN
                INSERT (Id, Username, AvatarUrl, LastSeenAt)
                VALUES (@Id, @Username, @AvatarUrl, @LastSeenAt);
            """;
            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, user);
        }

        public async Task UpdateAvatarAsync(Guid userId, string avatarUrl)
        {
            const string sql = "UPDATE ChatUsers SET AvatarUrl = @AvatarUrl WHERE Id = @UserId";
            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, new { UserId = userId, AvatarUrl = avatarUrl });
        }
    }
}
