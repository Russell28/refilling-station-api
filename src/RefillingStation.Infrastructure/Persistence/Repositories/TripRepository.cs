

using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.DTOs.Common;
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
                .ToListAsync();
        }

        public override async Task<Trip?> GetByIdAsync(int id)
        {
            return await _context.Trips
                .Include(t => t.Employee)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<int?> GetMaxTripNumberByDateAsync(DateOnly date)
        {
            return await _context.Trips
                .Where(x => x.Date == date)
                .MaxAsync(x => (int?)x.TripNumber);
             
        }

        public async Task<List<Trip>> SearchByDateRangeAsync(DateRangeOptions options)
        {
            const int PageSize = 50;

            var query = _context.Trips
                .AsNoTracking()
                .Include(x => x.Employee)
                .AsQueryable();

            if (options.StartDate.HasValue)
                query = query.Where(x => x.Date >= options.StartDate.Value);

            if (options.EndDate.HasValue)
                query = query.Where(x => x.Date <= options.EndDate.Value);

            return await query
                .OrderByDescending(x => x.Date)
                .ThenBy(t => t.TripNumber)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
