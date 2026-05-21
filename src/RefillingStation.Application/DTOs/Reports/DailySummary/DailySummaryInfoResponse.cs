namespace RefillingStation.Application.DTOs.Reports.DailySummary
{
    public sealed record DailySummaryInfoResponse(
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

        decimal TotalDebtCreatedToday,
        decimal TotalDebtPaymentsToday,
        decimal OutstandingDebt,

        decimal CashAfterExpense
    );
}
