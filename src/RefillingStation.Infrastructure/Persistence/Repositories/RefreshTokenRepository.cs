using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Enitities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(RefreshToken refreshToken)
            => await _context.RefreshTokens.AddAsync(refreshToken);

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
