namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed record DailyReportItem(
        DateOnly Date,

        decimal BacklogStartQty,
        decimal BacklogEndQty,

        int TripCount,

        decimal TotalCollectedQty,
        decimal TotalDeliveredQty,
        decimal TotalFreeQty,
        decimal TotalReturnedQty,
        decimal TotalReplacementQty,

        decimal TotalExpenses,
        decimal TotalPayrollEarned,
        decimal TotalPayrollPaid,

        decimal TotalDebtCreated,
        decimal TotalDebtPayment,

        decimal TotalCashCollected,
        decimal CashAfterExpense,
        decimal CashAfterPayroll
    );
}
