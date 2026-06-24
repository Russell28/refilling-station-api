using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

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

        public async Task<RefreshToken?> GetByTokenAsync(string token)
            => await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

        public async Task DeleteAllByUserIdAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId)
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(tokens);
        }
            
        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
