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
    public class RoomRepository : IRoomRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public RoomRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            const string sql = """
            SELECT Id, Name, Description, CreatedByUserId, CreatedAt
            FROM Rooms ORDER BY CreatedAt DESC
            """;
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Room>(sql);
        }

        public async Task<Room?> GetByIdAsync(Guid id)
        {
            const string sql = """
            SELECT Id, Name, Description, CreatedByUserId, CreatedAt
            FROM Rooms WHERE Id = @Id
            """;
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Room>(sql, new { Id = id });
        }

        public async Task<Guid> AddAsync(Room room)
        {
            const string sql = """
                INSERT INTO Rooms (Id, Name, Description, CreatedByUserId, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@Id, @Name, @Description, @CreatedByUserId, @CreatedAt)
                """;

            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<Guid>(sql, room);
        }

    }
}
