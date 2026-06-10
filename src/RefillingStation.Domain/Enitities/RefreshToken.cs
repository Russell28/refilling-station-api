namespace RefillingStation.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Token { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private init; }
        public DateTime? RevokedAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public User User { get; private set; } = null!;

        // Constructor enforces invariants
        public RefreshToken(int userId, string token, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty.", nameof(token));
            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Expiry must be in the future.", nameof(expiresAt));

            UserId = userId;
            Token = token;
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
            IsRevoked = false;
        }

        public void Revoke()
        {
            if (IsRevoked) return;
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
        }

        public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;

        public bool IsTokenRevoked() => IsRevoked;
    }
}
