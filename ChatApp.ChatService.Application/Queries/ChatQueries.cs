using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Application.Queries
{
    public record GetRoomHistoryQuery(Guid RoomId, int Page = 1, int PageSize = 50);

    public record GetAllRoomsQuery();
}
