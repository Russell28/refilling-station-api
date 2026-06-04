using RefillingStation.Application.DTOs.Auth;

namespace RefillingStation.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);

        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
    }
}
