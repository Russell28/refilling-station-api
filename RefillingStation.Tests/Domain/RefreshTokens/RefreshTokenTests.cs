using FluentAssertions;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;
using System.Security.Cryptography;

namespace RefillingStation.Tests.Domain.RefreshTokens
{
    public class RefreshTokenTests
    {
        #region Constructor
        [Fact]
        public void Constructor_Should_Create_RefreshToken_With_Valid_Data()
        {
            // Arrange
            var userId = 1;
            var token = new string('a', 40);
            var expiry = DateTime.UtcNow.AddDays(7);

            // Act
            var rt = new RefreshToken(userId, token, expiry);

            // Assert
            rt.UserId.Should().Be(userId);
            rt.Token.Should().Be(token);
            rt.ExpiresAt.Should().Be(expiry);
            rt.IsRevoked.Should().BeFalse();
            rt.CreatedAt.Should().BeBefore(DateTime.UtcNow);
            rt.RevokedAt.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Constructor_Should_Throw_When_Token_Is_Empty(string token)
        {
            // Arrange
            var builder = new RefreshTokenBuilder()
                .WithToken(token);

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.CommonCode.RequiredField);
        }

        [Fact]
        public void Constructor_Should_Throw_When_Token_Is_Invalid()
        {
            // Arrange
            var builder = new RefreshTokenBuilder()
                .WithToken(new string('a', 39));

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.RefreshTokenCode.InvalidToken);
        }

        [Fact]
        public void Constructor_Should_Throw_When_Expiry_Is_In_The_Past()
        {
            // Arrange
            var builder = new RefreshTokenBuilder()
                .WithExpiry(DateTime.UtcNow.AddDays(-1));

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.RefreshTokenCode.InvalidTimestamp);
        }
        #endregion

        #region Revoke
        [Fact]
        public void Revoke_Should_Set_IsRevoked_And_RevokedAt()
        {
            // Arrange
            var rt = new RefreshTokenBuilder().Build();

            // Act
            rt.Revoke();

            // Assert
            rt.IsRevoked.Should().BeTrue();
            rt.RevokedAt.Should().NotBeNull();
            rt.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Revoke_Should_Throw_When_Token_Is_Already_Revoked()
        {
            // Arrange
            var rt = new RefreshTokenBuilder().Build();

            rt.Revoke();

            // Act
            Action act = () => rt.Revoke();

            // Assert
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.RefreshTokenCode.AlreadyRevoked);
        }
        #endregion

        #region IsActive
        [Fact]
        public void IsActive_Should_Return_True_When_NotExpired_And_NotRevoked()
        {
            var rt = new RefreshTokenBuilder()
                .WithExpiry(DateTime.UtcNow.AddDays(1))
                .Build();

            rt.IsActive().Should().BeTrue();
        }

        [Fact]
        public void IsActive_Should_Return_False_When_Expired()
        {
            var rt = new RefreshTokenBuilder()
                .WithExpiry(DateTime.UtcNow.AddMilliseconds(100))
                .Build();

            // Wait until expiry has passed
            Thread.Sleep(200);

            rt.IsActive().Should().BeFalse();
        }

        [Fact]
        public void IsActive_Should_Return_False_When_Revoked()
        {
            var rt = new RefreshTokenBuilder()
                .Build();

            rt.Revoke();

            rt.IsActive().Should().BeFalse();
        }
        #endregion

        #region IsExpired
        [Fact]
        public void IsExpired_Should_Return_True_When_Expiry_Is_In_The_Past()
        {
            var rt = new RefreshTokenBuilder()
                .WithExpiry(DateTime.UtcNow.AddMilliseconds(100))
                .Build();

            // Wait until expiry has passed
            Thread.Sleep(200);

            rt.IsExpired().Should().BeTrue();
        }

        [Fact]
        public void IsExpired_Should_Return_False_When_Expiry_Is_In_The_Future()
        {
            var rt = new RefreshTokenBuilder()
                .WithExpiry(DateTime.UtcNow.AddDays(1))
                .Build();

            rt.IsExpired().Should().BeFalse();
        }
        #endregion


    }
}
