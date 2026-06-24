using RefillingStation.Domain.Enums;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string Username { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Relationships
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

        public User(
            string username,
            string passwordHash,
            UserRole role)
        {
            GuardAgainstInvalidUsername(username);
            GuardAgainstInvalidPassword(passwordHash);
            GuardAgainstInvalidRole(role);

            Username = username;
            PasswordHash = passwordHash;
            Role = role;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Domain Rules
        public void ChangePassword(string newHash)
        {
            GuardAgainstInvalidPassword(newHash);

            if (newHash == PasswordHash)
                throw new DomainException(DomainErrorCodes.UserCode.InvalidPassword, "New password cannot be the same as the old password.");

            PasswordHash = newHash;

            MarkAsUpdated();
        }

        public void ChangeRole(UserRole newRole)
        {
            GuardAgainstInvalidRole(newRole);

            if (newRole == Role)
                throw new DomainException(DomainErrorCodes.UserCode.InvalidUserRole, $"The user is already set to {newRole}");

            Role = newRole;

            MarkAsUpdated();
        }

        public void Activate()
        {
            if (IsActive)
                throw new DomainException(DomainErrorCodes.UserCode.AlreadyActive, "User is already active.");

            IsActive = true;

            MarkAsUpdated();
        }

        public void Deactivate()
        {
            if (!IsActive)
                throw new DomainException(DomainErrorCodes.UserCode.AlreadyInactive, "User is already inactive.");

            IsActive = false;

            MarkAsUpdated();
        }

        private void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        private void GuardAgainstInvalidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new DomainException(DomainErrorCodes.CommonCode.RequiredField, "Username is required.");
            if (username.Length < 5 || username.Length > 20)
                throw new DomainException(DomainErrorCodes.UserCode.InvalidUsernameLength, "Username must be between 5 and 20 characters.");
        }

        private void GuardAgainstInvalidPassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new DomainException(DomainErrorCodes.CommonCode.RequiredField, "Password is required.");
            if (passwordHash.Length < 50) // bcrypt hashes are ~60 chars
                throw new DomainException(DomainErrorCodes.UserCode.InvalidPassword, "Password hash is invalid.");
        }

        private void GuardAgainstInvalidRole(UserRole role)
        {
            if (role == default(UserRole) || !Enum.IsDefined(typeof(UserRole), role))
                throw new DomainException(DomainErrorCodes.UserCode.InvalidUserRole, "Role is invalid.");
        }
    }
}
