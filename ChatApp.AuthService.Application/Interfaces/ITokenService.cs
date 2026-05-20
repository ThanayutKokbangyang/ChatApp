using ChatApp.AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.AuthService.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();

        string GenerateCodeChallenge(string codeVerifier);

        ClaimsPrincipal? ValidateExpiredToken(string token);
    }
}
