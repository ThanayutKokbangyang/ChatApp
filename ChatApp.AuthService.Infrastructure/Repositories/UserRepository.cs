using ChatApp.AuthService.Domain.Entities;
using ChatApp.AuthService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ChatApp.AuthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            const string sql = """
                SELECT * Id, Email, Username, PasswordHash, Role, IsActive, CreatedAt
                FROM Users WHERE Id = @Id
                """;

            using var conn = _connectionFactory.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = """
                SELECT Id, Email, Username, PasswordHash, Role, IsActive, CreatedAt
                FROM Users WHERE Email = @Email
                """;
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Email = email.ToLowerInvariant() });
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            const string sql = """
                SELECT Id, Email, Username, PasswordHash, Role, IsActive, CreatedAt
                FROM Users WHERE Username = @Username
                """;
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Username = username.Trim() });
        }

        public async Task<Guid> AddAsync(User user)
        {
            const string sql = """
            INSERT INTO Users (Id, Email, Username, PasswordHash, Role, IsActive, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Id, @Email, @Username, @PasswordHash, @Role, @IsActive, @CreatedAt)
            """;

            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<Guid>(sql, user);
        }

        public async Task UpdateAsync(User user)
        {
            const string sql = """
                UPDATE Users SET Email = @Email, Username = @Username, PasswordHash = @PasswordHash,
                Role = @Role, IsActive = @IsActive WHERE Id = @Id
                """;
            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, user);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            const string sql = """
                SELECT COUNT(1) FROM Users WHERE Email = @Email
                """;
            using var conn = _connectionFactory.CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(sql, new { Email = email.ToLowerInvariant() });
            return count > 0;
        }

    }
}