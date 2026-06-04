using RefillingStation.Domain.Enitities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task DeleteAllByUserIdAsync(int  userId);
        Task<int> SaveChangesAsync();
    }
}
