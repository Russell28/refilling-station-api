namespace RefillingStation.Application.DTOs.Reports.DailySummary
{
    public record DailySummaryInfoResponse(
        DateOnly Date,

        decimal BacklogStartQty,
        decimal BacklogEndQty,

        int TripCount,
        decimal TotalCollectedQty,
        decimal TotalLoadedQty,
        decimal TotalDeliveredQty,
        decimal TotalFreeQty,
        decimal TotalReturnedQty,
        decimal TotalReplacementQty,

        decimal TotalCashCollected,
        decimal TotalExpenses,
        decimal TotalPayrollEarned,
        decimal TotalPayrollPaid,

        decimal TotalDebtCreatedToday,
        decimal TotalDebtPaymentsToday,
        decimal OutstandingDebt,

        decimal NetCashFlow
    );
}
