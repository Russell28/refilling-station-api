namespace RefillingStation.Application.DTOs.Reports.DailySummary
{
    public sealed record DailySummaryInfoResponse(
        DateOnly date,

        decimal backlogStartQty,
        decimal backlogEndQty,

        int tripCount,
        decimal collectedQtyTotal,
        decimal loadedQtyTotal,
        decimal deliveredQtyTotal,
        decimal freeQtyTotal,
        decimal returnedQtyTotal,
        decimal replacementQtyTotal,

        decimal cashCollectedTotal,
        decimal expensesTotal,

        decimal debtCreatedTodayTotal,
        decimal debtPaymentsTodayTotal,
        decimal outstandingDebt,

        decimal netBeforePayroll
    );
}
