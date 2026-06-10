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

        public User Build()
        {
            return new User(_username, _passwordHash, _role);
        }
    }
}
