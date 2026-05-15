using RefillingStation.Application.DTOs.MonthlyClosing;
using RefillingStation.Application.DTOs.Reports.MonthlySummary;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Repositories.Reports;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Services
{
    public class MonthlyClosingService : IMonthlyClosingService
    {
        private readonly IMonthlyClosingRepository _closingRepository;
        private readonly IMonthlySummaryRepository _summaryRepository;

        public MonthlyClosingService(
            IMonthlyClosingRepository closingRepository,
            IMonthlySummaryRepository summaryRepository)
        {
            _closingRepository = closingRepository;
            _summaryRepository = summaryRepository;
        }

        public async Task<MonthlySummaryResponse> CreateOrUpdateAsync(MonthlyClosingRequest request)
        {
            var (firstDay, lastDay) = ParseMonthYear(request.MonthYear);

            #region Summary
            var rawData = await _summaryRepository.GetMonthlySummaryAsync(firstDay, lastDay);

            var grossTotal = rawData.GrossTotal;
            var debtTotal = rawData.DebtTotal;
            var expenseTotal = rawData.ExpenseTotal;
            var payrollEarnedTotal = rawData.PayrollEarnedTotal;
            var payrollPaidTotal = rawData.PayrollPaidTotal;
            var payrollOwedTotal = payrollEarnedTotal - payrollPaidTotal;

            var netBeforePayroll = grossTotal - expenseTotal;
            var netAfterPayroll = grossTotal - expenseTotal - payrollEarnedTotal;
            var netCashFlow = grossTotal - expenseTotal - payrollPaidTotal;

            var summaryTotals = new MonthlySummaryTotals(
                grossTotal,
                debtTotal,
                expenseTotal,
                payrollEarnedTotal,
                payrollPaidTotal,
                payrollOwedTotal,

                netBeforePayroll,
                netAfterPayroll,
                netCashFlow
            );
            #endregion

            #region Closing
            var closing = await _closingRepository.GetByMonthYearAsync(request.MonthYear);

            if (closing is null) // Add
            {
                closing = new MonthlyClosing()
                {
                    Month = request.MonthYear,
                    TotalCashCollected = grossTotal,
                    TotalExpenses = expenseTotal,
                    TotalPayrollEarned = payrollEarnedTotal,
                    NetProfit = netAfterPayroll,
                    ManagerShare = request.ManagerShare,
                    OwnerShare = request.OwnerShare,
                    Notes = request.Notes,
                };

                await _closingRepository.AddMonthlyClosingAsync(closing);
            } else
            {
                closing.TotalCashCollected = grossTotal;
                closing.TotalExpenses = expenseTotal;
                closing.TotalPayrollEarned = payrollEarnedTotal;
                closing.NetProfit = netAfterPayroll;
                closing.ManagerShare = request.ManagerShare;
                closing.OwnerShare = request.OwnerShare;
                closing.Notes = request.Notes;
            }

            var rowsAffected = await _closingRepository.SaveChangesAsync();

            if (rowsAffected < 1)
                throw new Exception("Failed to create or update Monthly Closing");

            var savedClosing = new MonthlyClosingReportItem(
                closing.Month,
                closing.ManagerShare,
                closing.OwnerShare,
                closing.Notes,
                closing.CreatedAt
            );
            #endregion

            return new MonthlySummaryResponse(
                closing.Month,
                summaryTotals,
                savedClosing
            );
        }

        private (DateOnly FirstDay, DateOnly LastDay) ParseMonthYear(string monthYear)
        {
            if (!DateOnly.TryParse($"{monthYear}-01", out var firstDay))
                throw new ArgumentException("Invalid format. Use yyyy-MM.");

            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            return (firstDay, lastDay);
        }
    }
}
