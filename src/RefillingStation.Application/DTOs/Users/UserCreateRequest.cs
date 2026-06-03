using RefillingStation.Domain.Enums;

namespace RefillingStation.Application.DTOs.Users
{
    public sealed class UserCreateRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
