namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed record DashboardSummaryResponse(
        decimal TotalTrips,
        decimal BacklogStartQty,
        decimal TotalCollectedQty,
        decimal TotalLoadedQty,
        decimal TotalDeliveredQty,
        decimal BacklogEndQty,

        decimal TotalCashCollected,
        decimal TotalExpenses,
        decimal NetBeforePayroll,
        decimal TotalPayrollPaid,
        decimal NetCashFlow,

        decimal TotalDebtCreated,
        decimal TotalDebtPayments,
        decimal OutstandingDebt,

        decimal TotalSalaryEarned,
        decimal TotalPayrollOwed,
        decimal OutstandingPayroll
    );
}
