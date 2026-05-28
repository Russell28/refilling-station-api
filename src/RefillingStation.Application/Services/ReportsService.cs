using Microsoft.Extensions.Options;
using RefillingStation.Application.DTOs.Reports.Breakdowns;
using RefillingStation.Application.DTOs.Reports.DailySummary;
using RefillingStation.Application.DTOs.Reports.Dashboard;
using RefillingStation.Application.DTOs.Reports.MonthlySummary;
using RefillingStation.Application.Interfaces.Repositories.Reports;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Application.Settings;

namespace RefillingStation.Application.Services
{
    public class ReportsService : IReportsService
    {
        private readonly IDashboardRepository _dashboardRepository; 
        private readonly IDailySummaryRepository _dailySummaryRepository;
        private readonly IMonthlySummaryRepository _monthlySummaryRepository;
        private readonly BacklogSettings _backlogSettings;

        public ReportsService(
            IDashboardRepository dashboardRepository,
            IDailySummaryRepository dailySummaryRepository,
            IMonthlySummaryRepository monthlySummaryRepository,
            IOptions<BacklogSettings> options)
        {
            _dashboardRepository = dashboardRepository;
            _dailySummaryRepository = dailySummaryRepository;
            _monthlySummaryRepository = monthlySummaryRepository;
            _backlogSettings = options.Value;
        }
        public async Task<DashboardResponse> GetDashboardAsync(DateOnly startDate, DateOnly endDate)
        {
            var rawData = await _dashboardRepository.GetDashboardAsync(startDate, endDate);

            var trips = rawData.Trips;
            var debts = rawData.Debts;
            var expenses = rawData.Expenses;
            var payrolls = rawData.Payrolls;
            var tripsBefore = rawData.TripsBefore;
            var debtsRunning = rawData.DebtsRunning;
            var payrollsRunning = rawData.PayrollsRunning;

            // ---------------------------------------------------------
            // SUMMARY CORE
            // ---------------------------------------------------------
            #region Summary
            // Backlog Start
            var openingBacklogQty = _backlogSettings.OpeningBacklogQty;
            var previousCollectedQty = tripsBefore.Sum(x => x.CollectedQty);
            var previousDeliveredQty = tripsBefore.Sum(x => x.DeliveredQty);

            var backlogStartQty = openingBacklogQty + previousCollectedQty - previousDeliveredQty;

            // Trip Metrics
            var totalTrips = trips.Count;
            var totalCollectedQty = trips.Sum(x => x.CollectedQty);
            var totalDeliveredQty = trips.Sum(x => x.DeliveredQty);
            var totalLoadedQty = trips.Sum(x => x.LoadedQty);

            // Backlog End
            var backlogEndQty = backlogStartQty + totalCollectedQty - totalDeliveredQty;

            // Debt
            var totalDebtCreated = debts.Where(x => x.Amount > 0).Sum(x => x.Amount);
            var totalDebtPayments = debts.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount));
            var outstandingDebt = debtsRunning.Sum(x => x.Amount);

            // Payroll
            var totalPayrollEarned = payrolls.Sum(x => x.SalaryAmount);
            var totalPayrollPaid = payrolls.Sum(x => x.CashPaid);
            var outstandingPayroll =
                payrollsRunning.Sum(x => x.SalaryAmount) -
                payrollsRunning.Sum(x => x.CashPaid);

            // Expenses
            var totalExpenses = expenses.Sum(x => x.Amount);

            // Cashflow
            var totalCashCollected = trips.Sum(x => x.ActualCashCollected);
            var netBeforePayroll = totalCashCollected - totalExpenses;
            var netAfterPayroll = totalCashCollected - totalExpenses - totalPayrollEarned;
            #endregion

            // ---------------------------------------------------------
            // DAILY REPORTS
            // ---------------------------------------------------------
            #region DailyReports
            var dailyReports = new List<DailyReportItem>();

            var runningBacklogQty = backlogStartQty;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var backlogStartOfDay = runningBacklogQty;

                var tripsPerDay = trips.Where(x => x.Date == date).ToList();
                var expensesPerDay = expenses.Where(x => x.Date == date).ToList();
                var payrollsPerDay = payrolls.Where(x => x.EarnedDate == date).ToList();
                var debtsPerDay = debts.Where(x => x.Date == date).ToList();

                // Quantities
                var collectedQty = tripsPerDay.Sum(x => x.CollectedQty);
                var loadedQty = tripsPerDay.Sum(x => x.LoadedQty);
                var deliveredQty = tripsPerDay.Sum(x => x.DeliveredQty);
                var freeQty = tripsPerDay.Sum(x => x.FreeQty);
                var returnedQty = tripsPerDay.Sum(x => x.ReturnedQty);
                var replacementQty = tripsPerDay.Sum(x => x.ReplacementQty);

                // Debt
                var debtCreated = debtsPerDay.Where(x => x.Amount > 0).Sum(x => x.Amount);
                var debtPayments = debtsPerDay.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount));

                // Expenses & Payroll
                var expensesTotal = expensesPerDay.Sum(x => x.Amount);
                var payrollEarnedTotal = payrollsPerDay.Sum(x => x.SalaryAmount);
                var payrollPaidTotal = payrollsPerDay.Sum(x => x.CashPaid);

                // Trips
                var tripCount = tripsPerDay.Count;
                var cashCollected = tripsPerDay.Sum(x => x.ActualCashCollected);
                var cashAfterExpense = cashCollected - expensesTotal;
                var cashAfterPayroll = cashCollected - expensesTotal - payrollEarnedTotal;

                // Backlog
                runningBacklogQty += collectedQty - deliveredQty;

                if (cashCollected == 0 || deliveredQty == 0) // skip day off
                    continue;

                dailyReports.Add(new DailyReportItem(
                    date,

                    backlogStartOfDay,
                    runningBacklogQty,

                    tripCount,

                    collectedQty,
                    deliveredQty,
                    freeQty,
                    returnedQty,
                    replacementQty,

                    expensesTotal,
                    payrollEarnedTotal,
                    payrollPaidTotal,

                    debtCreated,
                    debtPayments,

                    cashCollected,
                    cashAfterExpense,
                    cashAfterPayroll
                ));
            }
            #endregion

            // ---------------------------------------------------------
            // BREAKDOWNS
            // ---------------------------------------------------------
            #region Breakdowns
            var expenseBreakdown = expenses
                .GroupBy(x => x.ExpenseCategoryId)
                .Select(g => new ExpenseBreakdownItemResponse(
                    g.Key,
                    g.First().ExpenseCategory,
                    g.Sum(x => x.Amount)
                ))
                .ToList();

            var expenseBreakdownResponse = new ExpenseBreakdownResponse(
                TotalExpense: expenseBreakdown.Sum(x => x.Amount),
                Items: expenseBreakdown
            );

            var debtBreakdown = debts
                .GroupBy(x => x.CustomerId)
                .Select(g => new DebtBreakdownItemResponse(
                    g.Key,
                    g.First().CustomerName,
                    g.Where(x => x.Amount > 0).Sum(x => x.Amount),
                    g.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount)),
                    g.Sum(x => x.Amount),
                    g.Max(x => x.Date)
                ))
                .Where(x => x.Balance != 0)
                .ToList();

            var debtBreakdownResponse = new DebtBreakdownResponse(
                TotalDebt: debtBreakdown.Sum(x => x.Balance),
                Items: debtBreakdown
            );

            var payrollBreakdown = payrolls
                .GroupBy(x => x.EmployeeId)
                .Select(g => new PayrollBreakdownItemResponse(
                    g.Key,
                    g.First().EmployeeName,
                    g.Sum(x => x.SalaryAmount),
                    g.Sum(x => x.CashPaid),
                    g.Sum(x => x.SalaryAmount) - g.Sum(x => x.CashPaid)
                ))
                .ToList();

            var payrollBreakdownResponse = new PayrollBreakdownResponse(
                TotalEarned: payrollBreakdown.Sum(x => x.Earned),
                TotalPaid: payrollBreakdown.Sum(x => x.Paid),
                TotalOwed: payrollBreakdown.Sum(x => x.Owed),
                Items: payrollBreakdown
            );
            #endregion

            // ---------------------------------------------------------
            // Cost, Price, Profit per gallon sold
            // ---------------------------------------------------------
            var costPerGal = (totalExpenses + totalPayrollEarned) / totalDeliveredQty;
            var retailPerGal = totalCashCollected / totalDeliveredQty;
            var profitPerGal = netAfterPayroll / totalDeliveredQty;

            // ---------------------------------------------------------
            // FINAL RESPONSE
            // ---------------------------------------------------------

            var summary = new DashboardSummaryResponse(
                backlogStartQty,
                backlogEndQty,

                totalTrips,
                totalCollectedQty,
                totalDeliveredQty,

                totalExpenses,

                totalPayrollEarned,
                totalPayrollPaid,
                outstandingPayroll,

                totalDebtCreated,
                totalDebtPayments,
                outstandingDebt,

                totalCashCollected,
                netBeforePayroll,
                netAfterPayroll,

                costPerGal,
                retailPerGal,
                profitPerGal
            );

            return new DashboardResponse(
                summary,
                dailyReports,
                expenseBreakdownResponse,
                debtBreakdownResponse,
                payrollBreakdownResponse
            );
        }


        public async Task<object> GetDailySummaryAsync(DateOnly date, bool isAdmin)
        {
            var rawData = await _dailySummaryRepository.GetDailySummaryAsync(date);

            var trips = rawData.Trips;
            var debts = rawData.Debts;
            var expenses = rawData.Expenses;
            var payrolls = rawData.Payrolls;
            var tripsBefore = rawData.TripsBefore;
            var debtsRunning = rawData.DebtsRunning;
            var payrollsRunning = rawData.PayrollsRunning;

            // ---------------------------------------------------------
            // SUMMARY CORE
            // ---------------------------------------------------------
            #region Summary
            // Backlog Start
            var openingBacklogQty = _backlogSettings.OpeningBacklogQty;
            var previousCollectedQty = tripsBefore.Sum(x => x.CollectedQty);
            var previousDeliveredQty = tripsBefore.Sum(x => x.DeliveredQty);

            var backlogStartQty = openingBacklogQty + previousCollectedQty - previousDeliveredQty;

            // Trip Metrics
            var totalTrips = trips.Count;
            var totalCollectedQty = trips.Sum(x => x.CollectedQty);
            var totalDeliveredQty = trips.Sum(x => x.DeliveredQty);
            var totalLoadedQty = trips.Sum(x => x.LoadedQty);
            var totalFreeQty = trips.Sum(x => x.FreeQty);
            var totalReturnedQty = trips.Sum(x => x.ReturnedQty);
            var totalReplacementQty = trips.Sum(x => x.ReplacementQty);

            // Backlog End
            var backlogEndQty = backlogStartQty + totalCollectedQty - totalDeliveredQty;

            // Debt
            var totalDebtCreated = debts.Where(x => x.Amount > 0).Sum(x => x.Amount);
            var totalDebtPayments = debts.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount));
            var outstandingDebt = debtsRunning.Sum(x => x.Amount);

            // Expenses
            var totalExpenses = expenses.Sum(x => x.Amount);

            // Cashflow
            var totalCashCollected = trips.Sum(x => x.ActualCashCollected);
            var cashAfterExpense = totalCashCollected - totalExpenses;
            #endregion

            // ---------------------------------------------------------
            // BREAKDOWNS
            // ---------------------------------------------------------
            #region Breakdowns
            var expenseBreakdown = expenses
                .GroupBy(x => x.ExpenseCategoryId)
                .Select(g => new ExpenseBreakdownItemResponse(
                    g.Key,
                    g.First().ExpenseCategory,
                    g.Sum(x => x.Amount)
                ))
                .ToList();

            var expenseBreakdownResponse = new ExpenseBreakdownResponse(
                TotalExpense: expenseBreakdown.Sum(x => x.Amount),
                Items: expenseBreakdown
            );

            var debtBreakdown = debtsRunning
                .GroupBy(x => x.CustomerId)
                .Select(g => new DebtBreakdownItemResponse(
                    g.Key,
                    g.First().CustomerName,
                    g.Where(x => x.Amount > 0).Sum(x => x.Amount),
                    g.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount)),
                    g.Sum(x => x.Amount),
                    g.Max(x => x.Date)
                ))
                .Where(x => x.Balance != 0)
                .ToList();

            var debtBreakdownResponse = new DebtBreakdownResponse(
                TotalDebt: debtBreakdown
                    .Where(x => x.Balance > 0)
                    .Sum(x => x.Balance),
                Items: debtBreakdown
            );

            // Payroll - Admin Only
            if (isAdmin)
            {
                var totalPayrollEarned = payrolls.Sum(x => x.SalaryAmount);
                var totalPayrollPaid = payrolls.Sum(x => x.CashPaid);
                var totalPayrollOwed = totalPayrollEarned - totalPayrollPaid;

                var outstandingPayroll =
                    payrollsRunning.Sum(x => x.SalaryAmount) -
                    payrollsRunning.Sum(x => x.CashPaid);

                var cashAfterPayroll = cashAfterExpense - totalPayrollEarned;

                var payrollBreakdown = payrollsRunning
                    .GroupBy(x => x.EmployeeId)
                    .Select(g => new PayrollBreakdownItemResponse(
                        g.Key,
                        g.First().EmployeeName,
                        g.Sum(x => x.SalaryAmount),
                        g.Sum(x => x.CashPaid),
                        g.Sum(x => x.SalaryAmount) - g.Sum(x => x.CashPaid)
                    ))
                    .Where(x => x.Owed != 0)
                    .ToList();

                var payrollBreakdownResponse = new PayrollBreakdownResponse(
                    TotalEarned: payrollBreakdown.Sum(x => x.Earned),
                    TotalPaid: payrollBreakdown.Sum(x => x.Paid),
                    TotalOwed: payrollBreakdown.Sum(x => x.Owed),
                    Items: payrollBreakdown
                );

                var adminSummary = new DailySummaryInfoAdminResponse(
                    date,

                    backlogStartQty,
                    backlogEndQty,

                    totalTrips,
                    totalCollectedQty,
                    totalLoadedQty,
                    totalDeliveredQty,
                    totalFreeQty,
                    totalReturnedQty,
                    totalReplacementQty,

                    totalCashCollected,
                    totalExpenses,
                    totalPayrollEarned,
                    totalPayrollPaid,

                    totalDebtCreated,
                    totalDebtPayments,
                    outstandingDebt,

                    cashAfterExpense,
                    cashAfterPayroll
                );

                return new DailySummaryAdminResponse(
                    adminSummary,
                    expenseBreakdownResponse,
                    debtBreakdownResponse,
                    payrollBreakdownResponse
                );
            }
            #endregion

            var summary = new DailySummaryInfoResponse(
                date,

                backlogStartQty,
                backlogEndQty,

                totalTrips,
                totalCollectedQty,
                totalLoadedQty,
                totalDeliveredQty,
                totalFreeQty,
                totalReturnedQty,
                totalReplacementQty,

                totalCashCollected,
                totalExpenses,

                totalDebtCreated,
                totalDebtPayments,
                outstandingDebt,

                cashAfterExpense
            );

            return new DailySummaryResponse(
                summary,
                expenseBreakdownResponse,
                debtBreakdownResponse
            );
        }

        public async Task<MonthlySummaryResponse> GetMonthlySummaryAsync(string monthYear)
        {
            var (firstDay, lastDay) = ParseMonthYear(monthYear);

            var rawData = await _monthlySummaryRepository.GetMonthlySummaryAsync(firstDay, lastDay);

            var grossTotal = rawData.GrossTotal;
            var debtTotal = rawData.DebtTotal;
            var expenseTotal = rawData.ExpenseTotal;
            var payrollEarnedTotal = rawData.PayrollEarnedTotal;
            var payrollPaidTotal = rawData.PayrollPaidTotal;
            var payrollOwedTotal = payrollEarnedTotal - payrollPaidTotal;
            var savedClosing = rawData.SavedClosing;

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

            return new MonthlySummaryResponse(
                monthYear,
                summaryTotals,
                rawData.SavedClosing
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
