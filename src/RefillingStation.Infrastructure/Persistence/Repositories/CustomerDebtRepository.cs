using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class CustomerDebtRepository : BaseRepository<CustomerDebtEntry>, ICustomerDebtRepository
    {
        public CustomerDebtRepository(AppDbContext context) : base(context) { }

        public override async Task<List<CustomerDebtEntry>> GetAllAsync()
            => await _context.CustomerDebtEntries
                .AsNoTracking()
                .Include(x => x.Customer)
                .ToListAsync();

        public override async Task<CustomerDebtEntry?> GetByIdAsync(int id)
            => await _context.CustomerDebtEntries
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<List<CustomerDebtEntry>> SearchByDateRangeAsync(DateRangeOptions options)
        {
            const int PageSize = 50;

            var query = _context.CustomerDebtEntries
                .AsNoTracking()
                .Include(x => x.Customer)
                .AsQueryable();

            if (options.StartDate.HasValue)
                query = query.Where(x => x.Date >= options.StartDate.Value);

            if (options.EndDate.HasValue)
                query = query.Where(x => x.Date <= options.EndDate.Value);

            return await query
                .OrderByDescending(x => x.Date)
                .Take(PageSize)
                .ToListAsync();
        }

    }
}
