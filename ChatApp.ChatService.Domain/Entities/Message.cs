using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; private set; }
        public Guid RoomId { get; private set; }
        public Guid SenderId { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public DateTime SentAt { get; private set; }
        public bool IsDeleted { get; private set; }

        private const int MaxContentLength = 4000;

        private Message() { }

        public static Message Create(Guid roomId, Guid senderId, string content)
        {
            if (roomId == Guid.Empty)
                throw new Exceptions.DomainException("RoomId is required");

            if (senderId == Guid.Empty)
                throw new Exceptions.DomainException("SenderId is required");

            if (string.IsNullOrWhiteSpace(content))
                throw new Exceptions.DomainException("Message content cannot be empty");

            if (content.Length > MaxContentLength)
                throw new Exceptions.DomainException(
                    $"Message cannot exceed {MaxContentLength} characters");

            return new Message
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsDeleted = false
            };


        }

        public void SoftDelete()
        {
            if (IsDeleted)
                throw new Exceptions.DomainException("Message is already deleted");
            IsDeleted = true;
        }

    }
}
