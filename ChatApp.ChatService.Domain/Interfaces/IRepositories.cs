using ChatApp.ChatService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Domain.Interfaces
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    // Repository Pattern — Message aggregate
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetByRoomAsync(Guid roomId, int page, int pageSize);
        Task<Message?> GetByIdAsync(Guid id);
        Task<Guid> AddAsync(Message message);
        Task UpdateAsync(Message message);
    }

    // Repository Pattern — Room aggregate
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(Guid id);
        Task<Guid> AddAsync(Room room);
    }

    // Repository Pattern — ChatUser (local copy of AuthService user)
    public interface IChatUserRepository
    {
        Task<ChatUser?> GetByIdAsync(Guid id);
        Task UpsertAsync(ChatUser user);
        Task UpdateAvatarAsync(Guid userId, string avatarUrl);
    }
}
