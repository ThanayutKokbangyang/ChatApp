using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.AuthService.Application.DTOs
{
    public record TokenResponseDto(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string TokenType = "Bearer"
    );

    public record UserDto(
        Guid Id,
        string Email,
        string Username,
        string Role
    );
}
