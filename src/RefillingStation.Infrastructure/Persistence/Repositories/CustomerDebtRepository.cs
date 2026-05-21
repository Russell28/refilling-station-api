using Microsoft.EntityFrameworkCore;
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
                .OrderByDescending(x => x.Date)
                .Take(50)
                .ToListAsync();

        public override async Task<CustomerDebtEntry?> GetByIdAsync(int id)
            => await _context.CustomerDebtEntries
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id);
    }
}
