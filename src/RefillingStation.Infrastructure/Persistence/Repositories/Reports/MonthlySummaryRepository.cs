using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.DTOs.Reports;
using RefillingStation.Application.DTOs.Reports.MonthlySummary;
using RefillingStation.Application.Interfaces.Repositories.Reports;

namespace RefillingStation.Infrastructure.Persistence.Repositories.Reports
{
    public class MonthlySummaryRepository : IMonthlySummaryRepository
    {
        private readonly AppDbContext _context;

        public MonthlySummaryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MonthlySummaryRawData> GetMonthlySummaryAsync(DateOnly start, DateOnly end)
        {
            var monthYear = start.ToString("yyyy-MM");

            var debtTotal = await _context.CustomerDebtEntries
                .Where(x =>
                    x.Date >= start
                    && x.Date <= end)
                .SumAsync(x => x.Amount);

            var expenseTotal = await _context.Expenses
                .Where(x =>
                    x.Date >= start
                    && x.Date <= end)
                .SumAsync (x => x.Amount);

            var payrollEarnedTotal = await _context.PayrollEntries
                .Where(x =>
                    x.EarnedDate >= start
                    && x.EarnedDate <= end)
                .SumAsync(x => x.SalaryAmount);

            var payrollPaidTotal = await _context.PayrollPayments
                .Where(x =>
                    x.PaidDate >= start
                    && x.PaidDate <= end)
                .SumAsync(x => x.AmountPaid);

            var grossTotal = await _context.Trips
                .Where(x => x.Date >= start && x.Date <= end)
                .SumAsync(x => x.ActualCashCollected);

            var savedClosing = await _context.MonthlyClosings
                .AsNoTracking()
                .Where(x => x.Month == monthYear)
                .Select(x => new MonthlyClosingReportItem (
                    x.Month,
                    x.ManagerShare,
                    x.OwnerShare,
                    x.Notes,
                    x.CreatedAt
                ))
                .FirstOrDefaultAsync();

            return new MonthlySummaryRawData
            {
                DebtTotal = debtTotal,
                ExpenseTotal = expenseTotal,
                PayrollEarnedTotal = payrollEarnedTotal,
                PayrollPaidTotal = payrollPaidTotal,
                GrossTotal = grossTotal,
                SavedClosing = savedClosing
            };
        }
    }
}
