using ChatApp.AuthService.Application.Commands;
using ChatApp.AuthService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.AuthService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterCommand command);
        Task<TokenResponseDto> LoginAsync(LoginCommand command);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenCommand command);
        Task RevokeTokenAsync(RevokeTokenCommand command);
    }
}
