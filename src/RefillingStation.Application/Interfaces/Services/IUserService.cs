using RefillingStation.Application.DTOs.Users;
using RefillingStation.Domain.Enums;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<List<UserDetailResponse>> GetAllAsync();
        Task<UserDetailResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(UserCreateRequest request);
        Task ChangePasswordAsync(int id, ChangePasswordRequest request);
        Task ChangeRoleAsync(int id, ChangeUserRoleRequest request);
        Task ActivateAsync(int id);
        Task DeactivateAsync(int targetUserId, int requestingUserId);
        Task DeleteAsync(int targetUserId, int requestingUserId);
    }
}
