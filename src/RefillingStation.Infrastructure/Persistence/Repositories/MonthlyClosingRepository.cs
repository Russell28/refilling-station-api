using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.DTOs.MonthlyClosing;
using RefillingStation.Application.DTOs.Reports.MonthlySummary;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class MonthlyClosingRepository : IMonthlyClosingRepository
    {
        private readonly AppDbContext _context;

        public MonthlyClosingRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<MonthlyClosing?> GetByMonthYearAsync(string monthYear)
        {
            return await _context.MonthlyClosings
                .FirstOrDefaultAsync(x => x.Month ==  monthYear);
        }

        public async Task AddMonthlyClosingAsync(MonthlyClosing entity)
        {
            await _context.MonthlyClosings.AddAsync(entity);
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
