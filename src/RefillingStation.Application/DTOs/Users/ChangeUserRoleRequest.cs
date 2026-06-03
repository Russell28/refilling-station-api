using RefillingStation.Domain.Enums;

namespace RefillingStation.Application.DTOs.Users
{
    public sealed class ChangeUserRoleRequest
    {
        public UserRole Role { get; set; }
    }
}
