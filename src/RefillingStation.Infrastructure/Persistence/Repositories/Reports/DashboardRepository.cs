using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.DTOs.Reports;
using RefillingStation.Application.DTOs.Reports.Dashboard;
using RefillingStation.Application.Interfaces.Repositories.Reports;

namespace RefillingStation.Infrastructure.Persistence.Repositories.Reports
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<DashboardRawData> GetDashboardAsync(DateOnly startDate, DateOnly endDate)
        {
            var debts = await _context.CustomerDebtEntries
                .AsNoTracking()
                .Where(x =>
                    x.Date >= startDate
                    && x.Date <= endDate)
                .Select(x => new CustomerDebtReportItem(
                    x.Id,
                    x.Date,
                    x.CustomerId,
                    x.Customer.Name,
                    x.Amount
                ))
                .ToListAsync();

            var expenses = await _context.Expenses
                .AsNoTracking()
                .Where(x =>
                    x.Date >= startDate
                    && x.Date <= endDate)
                .Select(x => new ExpenseReportItem(
                    x.Id,
                    x.Date,
                    x.ExpenseCategoryId,
                    x.Category.Name,
                    x.Amount
                ))
                .ToListAsync();

            var payrolls = await _context.PayrollEntries
                .AsNoTracking()
                .Where(x =>
                    x.EarnedDate >= startDate
                    && x.EarnedDate <= endDate)
                .Select(x => new PayrollReportItem(
                        x.Id,
                        x.EarnedDate,
                        x.EmployeeId,
                        x.Employee.FullName,
                        x.SalaryAmount,
                        x.CashPaid
                    ))
                .ToListAsync();

            var trips = await _context.Trips
                .AsNoTracking()
                .Where(x => 
                    x.Date >= startDate
                    && x.Date <= endDate)
                .Select(x => new TripReportItem(
                    x.Id,
                    x.Date,
                    x.TripNumber,
                    x.CollectedQty,
                    x.LoadedQty,
                    x.DeliveredQty,
                    x.FreeQty,
                    x.ReturnedQty,
                    x.ReplacementQty,
                    x.ActualCashCollected
                ))
                .ToListAsync();

            // Before start date
            var tripsBefore = await _context.Trips
                .AsNoTracking()
                .Where(x => x.Date < startDate)
                .Select(x => new TripReportItem(
                    x.Id,
                    x.Date,
                    x.TripNumber,
                    x.CollectedQty,
                    x.LoadedQty,
                    x.DeliveredQty,
                    x.FreeQty,
                    x.ReturnedQty,
                    x.ReplacementQty,
                    x.ActualCashCollected
                ))
                .ToListAsync();

            var debtsRunning = await _context.CustomerDebtEntries
                .AsNoTracking()
                .Where(x => x.Date <= endDate)
                .Select(x => new CustomerDebtReportItem(
                    x.Id,
                    x.Date,
                    x.CustomerId,
                    x.Customer.Name,
                    x.Amount
                ))
                .ToListAsync();

            var payrollsRunning = await _context.PayrollEntries
                .AsNoTracking()
                .Where(x => x.EarnedDate <= endDate)
                .Select(x => new PayrollReportItem(
                        x.Id,
                        x.EarnedDate,
                        x.EmployeeId,
                        x.Employee.FullName,
                        x.SalaryAmount,
                        x.CashPaid
                    ))
                .ToListAsync();

            return new DashboardRawData
            {
                Debts = debts,
                Expenses = expenses,
                Payrolls = payrolls,
                Trips = trips,
                TripsBefore = tripsBefore,
                DebtsRunning = debtsRunning,
                PayrollsRunning = payrollsRunning
            };
        }
    }
}
