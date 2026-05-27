
using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class ExpenseRepository : BaseRepository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(AppDbContext context) : base(context) { }

        public override async Task<List<Expense>> GetAllAsync()
        {
            return await _context.Expenses
                .AsNoTracking()
                .Include(x => x.Category)
                .ToListAsync();
        }

        public override async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Expense>> SearchByDateRangeAsync(DateRangeOptions options)
        {
            const int PageSize = 50;

            var query = _context.Expenses
                .AsNoTracking()
                .Include(x => x.Category)
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
