using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.AuthService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string Role { get; private set; } = "user";
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private User() { }

        public static User Create(string email, string username, string passwordHash, string role = "user")
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exceptions.DomainException("Email cannot be empty");

            if (string.IsNullOrWhiteSpace(username))
                throw new Exceptions.DomainException("Username cannot be empty");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new Exceptions.DomainException("Password hash cannot be empty");

            return new User
            {
                Id = Guid.NewGuid(),
                Email = email.ToLowerInvariant().Trim(),
                Username = username.Trim(),
                PasswordHash = passwordHash,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
