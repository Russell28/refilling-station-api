using RefillingStation.Application.DTOs.MonthlyClosing;
using RefillingStation.Application.DTOs.Reports.MonthlySummary;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IMonthlyClosingRepository
    {
        Task AddMonthlyClosingAsync(MonthlyClosing entity);
        Task<MonthlyClosing?> GetByMonthYearAsync(string monthYear);
        Task<int> SaveChangesAsync();
    }
}
