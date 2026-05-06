namespace RefillingStation.Application.DTOs.Reports.MonthlySummary
{
    public sealed record MonthlySummaryResponse(
        string Month,
        decimal TotalCashCollected,
        decimal TotalExpenses,
        decimal TotalPayrollEarned,
        decimal NetProfit,
        MonthlySavedClosingResponse? SavedClosing
    );
}
