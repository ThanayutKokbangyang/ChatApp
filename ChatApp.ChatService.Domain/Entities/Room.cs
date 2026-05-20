using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Domain.Entities
{
    public class Room
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Room() { }

        public static Room Create(string name, string? description, Guid createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exceptions.DomainException("Room name cannot be empty");

            if (name.Length > 100)
                throw new Exceptions.DomainException("Room name cannot exceed 100 characters");

            if (createdByUserId == Guid.Empty)
                throw new Exceptions.DomainException("CreatedByUserId is required");

            return new Room
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Description = description?.Trim(),
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}