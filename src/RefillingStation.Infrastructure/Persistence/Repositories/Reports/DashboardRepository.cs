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

            var payrollEntries = await _context.PayrollEntries
                .AsNoTracking()
                .Where(x =>
                    x.EarnedDate >= startDate
                    && x.EarnedDate <= endDate)
                .Select(x => new PayrollEntryReportItem(
                        x.Id,
                        x.EarnedDate,
                        x.EmployeeId,
                        x.Employee.FullName,
                        x.SalaryAmount
                    ))
                .ToListAsync();

            var payrollPayments = await _context.PayrollPayments
                .AsNoTracking()
                .Where(x =>
                    x.PaidDate >= startDate
                    && x.PaidDate <= endDate)
                .Select(x => new PayrollPaymentReportItem(
                        x.Id,
                        x.EmployeeId,
                        x.Employee.FullName,
                        x.PaidDate,
                        x.AmountPaid
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

            var payrollEntriesRunning = await _context.PayrollEntries
                .AsNoTracking()
                .Where(x => x.EarnedDate <= endDate)
                .Select(x => new PayrollEntryReportItem(
                        x.Id,
                        x.EarnedDate,
                        x.EmployeeId,
                        x.Employee.FullName,
                        x.SalaryAmount
                    ))
                .ToListAsync();

            var payrollPaymentsRunning = await _context.PayrollPayments
                .AsNoTracking()
                .Where(x => x.PaidDate <= endDate)
                .Select(x => new PayrollPaymentReportItem(
                        x.Id,
                        x.EmployeeId,
                        x.Employee.FullName,
                        x.PaidDate,
                        x.AmountPaid
                    ))
                .ToListAsync();

            return new DashboardRawData
            {
                Debts = debts,
                Expenses = expenses,
                PayrollEntries = payrollEntries,
                PayrollEntriesRunning = payrollEntriesRunning,
                PayrollPayments = payrollPayments,
                PayrollPaymentsRunning = payrollPaymentsRunning,
                Trips = trips,
                TripsBefore = tripsBefore,
                DebtsRunning = debtsRunning
            };
        }
    }
}
