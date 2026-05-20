using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.AuthService.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string? ReplacedBy { get; private set; }
        
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        private RefreshToken() { }

        public static RefreshToken Create(Guid userId, string token, TimeSpan lifetime)
        {
            if (userId == Guid.Empty)
                throw new Exceptions.DomainException("Token connot be empty");

            if (string.IsNullOrWhiteSpace(token))
                throw new Exceptions.DomainException("Token cannot be empty");

            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                ReplacedBy = token,
            };
        }
        public void Revoke(string? replacedByToken = null)
        {
            IsRevoked = true;
            ReplacedBy = replacedByToken;
        }

    }
}
