using ChatApp.ChatService.Domain.Entities;
using ChatApp.ChatService.Domain.Interfaces;
using Dapper;

namespace ChatApp.ChatService.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MessageRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Message>> GetByRoomAsync(Guid roomId, int page, int pageSize)
        {
            const string sql = """
                SELECT Id, RoomId, SenderId, Content, MessageType, SentAt, IsDeleted
                FROM Messages
                WHERE RoomId = @RoomId AND IsDeleted = 0
                ORDER BY SentAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                """;

            using var conn = _connectionFactory.CreateConnection();

            return await conn.QueryAsync<Message>(sql, new
            {
                RoomId = roomId,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });
        }

        public async Task<Message?> GetByIdAsync(Guid id)
        {
            const string sql = """
                SELECT Id, RoomId, SenderId, Content, MessageType, SentAt, IsDeleted
                FROM Messages
                WHERE Id = @Id
                """;

            using var conn = _connectionFactory.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Message>(sql, new { Id = id });
        }

        public async Task<Guid> AddAsync(Message message)
        {
            const string sql = """
                INSERT INTO Messages(Id, RoomId, SenderId, Content, MessageType, SentAt, IsDeleted)
                OUTPUT INSERTED.Id
                VALUES(@Id, @RoomId, @SenderId, @Content, @MessageType, @SentAt, @IsDeleted)
                """;

            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<Guid>(sql, message);
        }

        public async Task UpdateAsync(Message message)
        {
            const string sql = "UPDATE Messages SET IsDeleted = @IsDeleted WHERE Id = @Id";

            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, new { message.Id, message.IsDeleted });
        }
    }
}