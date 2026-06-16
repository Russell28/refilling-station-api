using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Enums;

namespace RefillingStation.Tests.Domain.Users
{
    public class UserBuilder
    {
        private string _username = "admin01";
        // Fake bcrypt hash (just needs to satisfy validation length)
        private string _passwordHash = "$2a$12$abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234";
        private UserRole _role = UserRole.Admin;

        private int _id = 1;
        private bool _isActive = true;

        public UserBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public UserBuilder WithPasswordHash(string passwordHash)
        {
            _passwordHash = passwordHash;
            return this;
        }

        public UserBuilder WithRole(UserRole role)
        {
            _role = role;
            return this;
        }

        public UserBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public UserBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public User Build()
        {
            // Use the public constructor for required fields
            var user = new User(_username, _passwordHash, _role);

            // Set private properties via reflection
            typeof(User).GetProperty(nameof(User.Id))!
                .SetValue(user, _id);
            typeof(User).GetProperty(nameof(User.IsActive))!
                .SetValue(user, _isActive);

            return user;
        }
    }
}
