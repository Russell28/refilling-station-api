

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RefillingStation.Application.DTOs.Auth;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Application.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Tests.Domain.Builders;

namespace RefillingStation.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository;
        private readonly Mock<IValidator<LoginRequest>> _validator;
        private readonly Mock<ITokenService> _tokenService;
        private readonly Mock<IPasswordHasher> _passwordHasher;

        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepository = new Mock<IUserRepository>();
            _refreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _validator = new Mock<IValidator<LoginRequest>>();
            _tokenService = new Mock<ITokenService>();
            _passwordHasher = new Mock<IPasswordHasher>();

            _authService = new AuthService(
                    _userRepository.Object,
                    _refreshTokenRepository.Object,
                    _validator.Object,
                    _tokenService.Object,
                    _passwordHasher.Object
                );
        }

        #region LoginAsync
        [Fact]
        public async Task LoginAsync_Should_Return_Tokens_And_Save_RefreshToken_When_Credentials_Are_Valid()
        {
            // Arrange
            var request = new LoginRequest()
            {
                Username = "Admin01",
                Password = "Password123!"
            };

            _validator
                .Setup(x => x.ValidateAsync(
                    request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var user = new UserBuilder()
                .WithId(1)
                .Build();

            _userRepository
                .Setup(r => r.GetByUsernameAsync(request.Username))
                .ReturnsAsync(user);

            _passwordHasher
                .Setup(x => x.Verify(request.Password, user.PasswordHash))
                .Returns(true);

            var validAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6ImFkbWluIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3ODEyNTkxMTMsImlzcyI6InJlZmlsbGluZy1zdGF0aW9uLWFwaSIsImF1ZCI6InJlZmlsbGluZy1zdGF0aW9uLWFwcCJ9.N23ka2fpUJRJfwDFOXYa4SxnVv0L8Tmrr3bWIzC1dtE";

            _tokenService
                .Setup(x => x.GenerateAccessToken(user))
                .Returns(validAccessToken);

            var validRefreshToken = "d%2BXLgkYK7HJ9xX8yYuoy3ilBPE7gvWHwwt%2F2r4IBVYo%3D";

            _tokenService
                .Setup(x => x.GenerateRefreshToken())
                .Returns(validRefreshToken);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().Be(validAccessToken);
            result.RefreshToken.Should().Be(validRefreshToken);

            // Important: don't verify against a specific instance (rt) because the service
            // creates a new RefreshToken object internally. Moq compares references by default,
            // so this would fail even if property values match. Instead, use It.Is<T> to match
            // on the values of the properties you care about (UserId, Token, ExpiresAt, etc.).
            // This pattern should be reused in future tests whenever the code under test
            // constructs new objects that you can't directly reference.
            _refreshTokenRepository.Verify(r => r.AddAsync(
                It.Is<RefreshToken>(x =>
                    x.UserId == 1 &&
                    x.Token == validRefreshToken &&
                    x.ExpiresAt.Date == DateTime.UtcNow.AddDays(7).Date
                )
            ), Times.Once());

            _refreshTokenRepository
                .Verify(r => r.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new LoginRequest();

            var validationResult = new ValidationResult(
                new[]
                {
                    new ValidationFailure("Username", "Required")
                });

            _validator
                .Setup(x => x.ValidateAsync(
                    request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            Func<Task> act = () => _authService.LoginAsync(request);
            
            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_When_User_NotFound()
        {
            // Arrange
            var request = new LoginRequest();

            _validator
                .Setup(x => x.ValidateAsync(
                    request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _userRepository
                .Setup(r => r.GetByUsernameAsync("admin01"))
                .ReturnsAsync((User?) null);

            // Act
            Func<Task> act = () => _authService.LoginAsync(request);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_When_Credentials_Are_Invalid()
        {
            // Arrange
            var request = new LoginRequest();

            _validator
                .Setup(x => x.ValidateAsync(
                    request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var user = new UserBuilder().Build();

            _userRepository
                .Setup(r => r.GetByUsernameAsync("admin01"))
                .ReturnsAsync(user);

            _passwordHasher
                .Setup(x => x.Verify("Password123", user.PasswordHash))
                .Returns(false);

            // Act
            Func<Task> act = () => _authService.LoginAsync(request);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_When_User_Is_Inactive()
        {
            // Arrange
            var request = new LoginRequest();

            _validator
                .Setup(x => x.ValidateAsync(
                    request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var user = new UserBuilder()
                .WithIsActive(false)
                .Build();

            _userRepository
                .Setup(r => r.GetByUsernameAsync("admin01"))
                .ReturnsAsync(user);

            _passwordHasher
                .Setup(x => x.Verify("Password123", user.PasswordHash))
                .Returns(true);

            // Act
            Func<Task> act = () => _authService.LoginAsync(request);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
        #endregion

        #region RefreshTokenAsync
        [Fact]
        public async Task RefreshTokenAsync_Should_Revoke_Old_And_Save_New_Token_And_Return_Access_And_Refresh()
        {
            var refreshToken = "d%2BXLgkYK7HJ9xX8yYuoy3ilBPE7gvWHwwt%2F2r4IBVYo%3D";

            var refreshTokenEntity = new RefreshToken(
                    1,
                    refreshToken,
                    DateTime.UtcNow.AddDays(7)
                );

            var user = new UserBuilder().Build();

            // Attach navigation manually for test purposes
            typeof(RefreshToken)
                .GetProperty(nameof(RefreshToken.User))!
                .SetValue(refreshTokenEntity, user);

            _refreshTokenRepository
                .Setup(r => r.GetByTokenAsync(refreshToken))
                .ReturnsAsync(refreshTokenEntity);

            var newAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6ImFkbWluIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3ODEyNTkxMTMsImlzcyI6InJlZmlsbGluZy1zdGF0aW9uLWFwaSIsImF1ZCI6InJlZmlsbGluZy1zdGF0aW9uLWFwcCJ9.N23ka2fpUJRJfwDFOXYa4SxnVv0L8Tmrr3bWIzC1dtE";
            var newRefreshToken = "d%2BXLgkYK7HJ9xX8yYuoy3ilBPE7gvWHwwt%2F2r4IBVYo%3D";

            _tokenService
                .Setup(x => x.GenerateAccessToken(user))
                .Returns(newAccessToken);

            _tokenService
                .Setup(x => x.GenerateRefreshToken())
                .Returns(newRefreshToken);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().Be(newAccessToken);
            result.RefreshToken.Should().Be(newRefreshToken);

            _refreshTokenRepository
                .Verify(r => r.AddAsync(
                    It.Is<RefreshToken>(x => 
                        x.UserId == 1 &&
                        x.Token == newRefreshToken &&
                        x.ExpiresAt.Date == DateTime.UtcNow.AddDays(7).Date)),
                    Times.Once);

            _refreshTokenRepository
                .Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_Should_Throw_When_Token_NotFound()
        {
            // Arrange
            var token = "d%2BXLgkYK7HJ9xX8yYuoy3ilBPE7gvWHwwt%2F2r4IBVYo%3D";

            _refreshTokenRepository
                .Setup(r => r.GetByTokenAsync(token))
                .ReturnsAsync((RefreshToken?)null);

            // Act
            Func<Task> act = () => _authService.RefreshTokenAsync(token);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task RefreshTokenAsync_Should_Throw_When_Inactive()
        {
            // Arrange
            var token = "d%2BXLgkYK7HJ9xX8yYuoy3ilBPE7gvWHwwt%2F2r4IBVYo%3D";

            var refreshToken = new RefreshToken(
                    1,
                    token,
                    DateTime.UtcNow.AddMilliseconds(100)
                );

            _refreshTokenRepository
                .Setup(r => r.GetByTokenAsync(token))
                .ReturnsAsync(refreshToken);

            // Act
            // Wait until expiry has passed
            Thread.Sleep(200);
            Func<Task> act = () => _authService.RefreshTokenAsync(token);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task RefreshTokenAsync_Should_Throw_When_User_NotFound()
        {
            // Arrange
            var token = "d%2BXLgkYK7HJ9xX8yYuoy3ilBPE7gvWHwwt%2F2r4IBVYo%3D";

            var refreshToken = new RefreshToken(
                    1,
                    token,
                    DateTime.UtcNow.AddDays(7)
                );

            _refreshTokenRepository
                .Setup(r => r.GetByTokenAsync(token))
                .ReturnsAsync(refreshToken);

            // Act
            Func<Task> act = () => _authService.RefreshTokenAsync(token);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
        #endregion

        #region LogoutAsync
        [Fact]
        public async Task LogoutAsync_Should_Delete_User_RefreshTokens()
        {
            await _authService.LogoutAsync(1);

            _refreshTokenRepository.Verify(x => x.DeleteAllByUserIdAsync(1), Times.Once());
            _refreshTokenRepository.Verify(x => x.SaveChangesAsync(), Times.Once());
        }
        #endregion
    }
}
