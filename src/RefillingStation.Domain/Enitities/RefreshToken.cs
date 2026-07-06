using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private init; }
        public DateTime? RevokedAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public User User { get; private set; } = null!;

        protected RefreshToken() { } // EF Core

        // Constructor enforces invariants
        public RefreshToken(int userId, string token, DateTime expiresAt)
        {
            if (userId <= 0)
                throw new DomainException(
                    DomainErrorCodes.CommonCode.RequiredField,
                    "UserId is required.");

            if (string.IsNullOrWhiteSpace(token))
                throw new DomainException(
                    DomainErrorCodes.CommonCode.RequiredField, 
                    "Token is required.");

            if (token.Length < 40)
                throw new DomainException(
                    DomainErrorCodes.RefreshTokenCode.InvalidToken,
                    "Refresh token format is invalid.");

            if (expiresAt <= DateTime.UtcNow)
                throw new DomainException(
                    DomainErrorCodes.RefreshTokenCode.InvalidTimestamp,
                    "Expiry must be in the future.");

            UserId = userId;
            Token = token;
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
            IsRevoked = false;
        }

        public void Revoke()
        {
            if (IsRevoked)
                throw new DomainException(
                    DomainErrorCodes.RefreshTokenCode.AlreadyRevoked,
                    "Token is already revoked.");

            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
        }

        public bool IsActive() => !IsExpired() && !IsRevoked;

        public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;

        //public bool IsTokenRevoked() => IsRevoked;
    }
}
