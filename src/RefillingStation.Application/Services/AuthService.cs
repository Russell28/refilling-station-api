using FluentValidation;
using RefillingStation.Application.DTOs.Auth;
using RefillingStation.Application.Interfaces;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;

namespace RefillingStation.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;
        private readonly IValidator<LoginRequest> _validator;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(
            IAuthRepository repository, 
            IValidator<LoginRequest> validator,
            ITokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _repository = repository;
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
            var user = await _repository.GetByUsernameAsync(request.Username);

            if (user is null)
                throw new UnauthorizedAccessException("Invalid Credentials");

            // 3. Check Password
            var valid = _passwordHasher.Verify(request.Password, user.PasswordHash);

            if (!valid)
                throw new UnauthorizedAccessException("Invalid Credentials");

            // 4. Generate token
            var token = _tokenService.GenerateAccessToken(user);

            return new LoginResponse
            (
                token,
                user.Username,
                user.Role
            );
        }
    }
}
