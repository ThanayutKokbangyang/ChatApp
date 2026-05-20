using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.AuthService.Application.Commands
{
    public record RegisterCommand(string Email, string Username, string Password);
    public record LoginCommand(string Email, string Password);
    public record RefreshTokenCommand(string AccessToken, string RefreshToken);
    public record RevokeTokenCommand(string RefreshToken);
   
}
