namespace RefillingStation.Application.DTOs.Reports
{
    public sealed record DailyReportItem(
        DateOnly Date,

        decimal TripCount,
        decimal CollectedQty,
        decimal LoadedQty,
        decimal DeliveredQty,
        decimal FreeQty,
        decimal ReturnedQty,
        decimal ReplacementQty,

        decimal CashCollected,
        decimal Expenses,
        decimal PayrollEarned,
        decimal PayrollPaid,
        decimal NetCashFlow,

        decimal DebtCreated,
        decimal DebtPayments,

        decimal BacklogStartQty,
        decimal BacklogEndQty
    );
}
