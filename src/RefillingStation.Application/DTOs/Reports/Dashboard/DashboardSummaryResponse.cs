namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed record DashboardSummaryResponse(
        decimal BacklogStartQty,
        decimal BacklogEndQty,

        int WorkedDaysCount,
        int TripCount,
        decimal CollectedQtyTotal,
        decimal DeliveredQtyTotal,

        decimal ExpensesTotal,
        decimal ExpensesDailyAverage,

        decimal PayrollEarnedTotal,
        decimal PayrollPaidTotal,
        decimal OutstandingPayroll,

        decimal DebtCreatedTotal,
        decimal DebtPaymentsTotal,
        decimal OutstandingDebt,

        decimal CashCollectedTotal,
        decimal NetBeforePayroll,
        decimal NetAfterPayroll,
        decimal NetCashFlow,

        decimal CostPerGal,
        decimal RetailPerGal,
        decimal ProfitPerGal,
        decimal SalaryPaidPerGal
    );
}
