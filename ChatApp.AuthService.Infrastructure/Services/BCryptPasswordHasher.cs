using ChatApp.AuthService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.AuthService.Infrastructure.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        public string Hash(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        public bool Verify(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Handle invalid hash format
                return false;
            }

        }
    }
}
