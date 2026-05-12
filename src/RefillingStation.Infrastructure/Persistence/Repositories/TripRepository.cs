

using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class TripRepository : BaseRepository<Trip>, ITripRepository
    {
        public TripRepository(AppDbContext context) : base(context) { }

        public override async Task<List<Trip>> GetAllAsync()
        {
            return await _context.Trips
                .AsNoTracking()
                .Include(t => t.Employee)
                .OrderByDescending(t => t.Date)
                .ThenBy(t => t.TripNumber)
                .Take(50)
                .ToListAsync();
        }

        public override async Task<Trip?> GetByIdAsync(int id)
        {
            return await _context.Trips
                .AsNoTracking()
                .Include(t => t.Employee)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
