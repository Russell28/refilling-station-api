namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed record DashboardSummaryResponse(
        decimal BacklogStartQty,
        decimal BacklogEndQty,

        decimal TotalTrips,
        decimal TotalCollectedQty,
        decimal TotalDeliveredQty,

        decimal TotalExpense,

        decimal TotalPayrollEarned,
        decimal TotalPayrollPaid,
        decimal OutstandingPayroll,

        decimal TotalDebtCreated,
        decimal TotalDebtPayments,
        decimal OutstandingDebt,

        decimal TotalCashCollected,
        decimal NetAfterExpense,
        decimal NetAfterPayroll,

        decimal CostPerGallon,
        decimal RetailPerGallon,
        decimal ProfitPerGallon
    );
}
