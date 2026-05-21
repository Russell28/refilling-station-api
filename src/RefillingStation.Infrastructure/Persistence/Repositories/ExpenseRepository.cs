
using Microsoft.EntityFrameworkCore;
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
                .OrderByDescending(x => x.Date)
                .Take(50)
                .ToListAsync();
        }

        public override async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
