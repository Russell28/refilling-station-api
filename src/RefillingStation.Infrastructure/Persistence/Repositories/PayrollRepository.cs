using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class PayrollRepository : BaseRepository<PayrollEntry>, IPayrollRepository
    {
        public PayrollRepository(AppDbContext context) : base(context) { }

        public override async Task<List<PayrollEntry>> GetAllAsync()
        {
            return await _context.PayrollEntries
                .AsNoTracking()
                .Include(e => e.Employee)
                .ToListAsync();
        }

        public override async Task<PayrollEntry?> GetByIdAsync(int id)
        {
            return await _context.PayrollEntries
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<PayrollEntry>> SearchByDateRangeAsync(DateRangeOptions options)
        {
            const int PageSize = 50;

            var query = _context.PayrollEntries
                .AsNoTracking()
                .Include(x => x.Employee)
                .AsQueryable();

            if (options.StartDate.HasValue)
                query = query.Where(x => x.EarnedDate >= options.StartDate.Value);

            if (options.EndDate.HasValue)
                query = query.Where(x => x.EarnedDate <= options.EndDate.Value);

            return await query
                .OrderByDescending(x => x.EarnedDate)
                .ThenBy(p => p.Employee.FirstName)
                .ThenBy(p => p.Employee.LastName)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
