using RefillingStation.Domain.Entities;
using System.Security.Cryptography;

namespace RefillingStation.Tests.Domain.Builders
{
    /// <summary>
    /// Creates valid RefreshToken instances for tests.
    /// </summary>
    public class RefreshTokenBuilder
    {
        private int _userId = 1;
        private string _token = GenerateSecureToken();
        private DateTime _expiresAt = DateTime.UtcNow.AddDays(7);

        public RefreshTokenBuilder WithUserId(int userId)
        {
            _userId = userId;
            return this;
        }

        public RefreshTokenBuilder WithToken(string token)
        {
            _token = token;
            return this;
        }

        public RefreshTokenBuilder WithExpiry(DateTime expiry)
        {
            _expiresAt = expiry;
            return this;
        }

        public RefreshToken Build()
        {
            return new RefreshToken(
                    _userId,
                    _token,
                    _expiresAt);
        }

        private static string GenerateSecureToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
