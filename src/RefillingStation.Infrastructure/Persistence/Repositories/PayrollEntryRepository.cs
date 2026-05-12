using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class PayrollEntryRepository : BaseRepository<PayrollEntry>, IPayrollRepository
    {
        public PayrollEntryRepository(AppDbContext context) : base(context) { }

        public override async Task<List<PayrollEntry>> GetAllAsync()
        {
            return await _context.PayrollEntries
                .AsNoTracking()
                .Include(e => e.Employee)
                .OrderByDescending(x => x.EarnedDate)
                .Take(100)
                .ToListAsync();
        }

        public override async Task<PayrollEntry?> GetByIdAsync(int id)
        {
            return await _context.PayrollEntries
                .AsNoTracking()
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
