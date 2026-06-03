using FluentValidation;
using RefillingStation.Application.DTOs.Auth;
using RefillingStation.Application.Interfaces;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Enitities;

namespace RefillingStation.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IValidator<LoginRequest> _validator;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IValidator<LoginRequest> validator,
            ITokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _validator = validator;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // 1. Validate the request
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            // 2. Get User
            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if (user is null)
                throw new UnauthorizedAccessException("Invalid Credentials");

            // 3. Check Password
            var valid = _passwordHasher.Verify(request.Password, user.PasswordHash);

            if (!valid)
                throw new UnauthorizedAccessException("Invalid Credentials");

            // 4. Check if user is active
            if (!user.IsActive)
                throw new UnauthorizedAccessException("User account is inactive.");

            // 5. Generate token
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken(
                    user.Id,
                    refreshToken,
                    DateTime.UtcNow.AddDays(7)
                );

            // 6. Save RefreshToken to DB
            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            return new LoginResponse
            (
                accessToken,
                refreshToken,
                user.Username,
                user.Role.ToString()
            );
        }
    }
}
