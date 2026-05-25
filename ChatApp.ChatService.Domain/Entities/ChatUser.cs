using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Domain.Entities
{
    public class ChatUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime LastSeenAt { get; set; }
    }
}
