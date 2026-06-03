using RefillingStation.Domain.Enitities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken);
        Task<int> SaveChangesAsync();
    }
}
