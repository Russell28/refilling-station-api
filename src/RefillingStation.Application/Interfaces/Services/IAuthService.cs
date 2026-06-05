using RefillingStation.Application.DTOs.Auth;

namespace RefillingStation.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(int userId);
    }
}
