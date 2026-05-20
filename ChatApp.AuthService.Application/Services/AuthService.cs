using ChatApp.AuthService.Application.Commands;
using ChatApp.AuthService.Application.DTOs;
using ChatApp.AuthService.Application.Interfaces;
using ChatApp.AuthService.Domain.Entities;
using ChatApp.AuthService.Domain.Exceptions;
using ChatApp.AuthService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;


namespace ChatApp.AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

        private const int AccessTokenLifetimeSeconds = 900;

        public AuthService(IUserRepository userRepo, IRefreshTokenRepository refreshTokenRepo, ITokenService tokenService, IPasswordHasher passwordHasher)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserDto> RegisterAsync(RegisterCommand command)
        {
            if (await _userRepo.ExistsByEmailAsync(command.Email))
                throw new DomainException("Email already in use.");

            var passwordHash = _passwordHasher.Hash(command.Password);
            var user = User.Create(command.Email, command.Username, passwordHash);

            await _userRepo.AddAsync(user);

            return new UserDto(
                user.Id,
                user.Email,
                user.Username,
                user.Role
            );
        }

        public async Task<TokenResponseDto> LoginAsync(LoginCommand command)
        {
            var user = await _userRepo.GetByEmailAsync(command.Email) ?? throw new DomainException("Invalid credentials");

            if (!user.IsActive)
                throw new DomainException("Account is deactivated");

            if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
                throw new DomainException("Invalid credentials");

            return await GenerateTokensAsync(user);
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenCommand command)
        {
            var principal = _tokenService.ValidateExpiredToken(command.AccessToken) 
                ?? throw new DomainException("Invalid access token.");

            var storedToken = await _refreshTokenRepo.GetByTokenAsync(command.RefreshToken);
            if (storedToken is null || !storedToken.IsActive)
                throw new DomainException("Invalid or expired refresh token");

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? throw new DomainException("Invalid token claims.");

            var userId = Guid.Parse(userIdClaim);
            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new DomainException("User not found.");

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
            var newRefreshToken = RefreshToken.Create(user.Id, newRefreshTokenValue, RefreshTokenLifetime);

            storedToken.Revoke(replacedByToken: newRefreshTokenValue);
            await _refreshTokenRepo.UpdateAsync(storedToken);
            await _refreshTokenRepo.AddAsync(newRefreshToken);

            return new TokenResponseDto(newAccessToken, newRefreshTokenValue, AccessTokenLifetimeSeconds);
        }

        public async Task RevokeTokenAsync(RevokeTokenCommand command)
        {
            var token = await _refreshTokenRepo.GetByTokenAsync(command.RefreshToken);
            if (token is null) return;

            token.Revoke();
            await _refreshTokenRepo.UpdateAsync(token);
        }

        private async Task<TokenResponseDto> GenerateTokensAsync(User user)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            var refreshToken = RefreshToken.Create(user.Id, refreshTokenValue, RefreshTokenLifetime);

            await _refreshTokenRepo.AddAsync(refreshToken);

            return new TokenResponseDto(accessToken, refreshTokenValue, AccessTokenLifetimeSeconds);
        }

        }
}
