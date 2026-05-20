using ChatApp.AuthService.Domain.Entities;
using ChatApp.AuthService.Domain.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.AuthService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public RefreshTokenRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            const string sql = """
            SELECT Id, UserId, Token, ExpiresAt, IsRevoked, CreatedAt, ReplacedBy
            FROM RefreshTokens WHERE Token = @Token
            """;

            using var conn = _connectionFactory.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            const string sql = """
            INSERT INTO RefreshTokens (Id, UserId, Token, ExpiresAt, IsRevoked, CreatedAt, ReplacedBy)
            VALUES (@Id, @UserId, @Token, @ExpiresAt, @IsRevoked, @CreatedAt, @ReplacedBy)
            """;

            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, refreshToken);
        }

        public async Task UpdateAsync(RefreshToken refreshToken)
        {
            const string sql = """
            UPDATE RefreshTokens
            SET IsRevoked = @IsRevoked, ReplacedBy = @ReplacedBy
            WHERE Id = @Id
            """;

            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, refreshToken);
        }

        public async Task RevokeAllForUserAsync(Guid userId)
        {
            const string sql = """
            UPDATE RefreshTokens SET IsRevoked = 1
            WHERE UserId = @UserId AND IsRevoked = 0
            """;

            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, new { UserId = userId });
        }
    }
}
