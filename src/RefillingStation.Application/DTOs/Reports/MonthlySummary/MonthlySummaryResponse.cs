namespace RefillingStation.Application.DTOs.Reports.MonthlySummary
{
    public sealed record MonthlySummaryResponse(
        string MonthYear,
        MonthlySummaryTotals SummaryTotals,
        MonthlyClosingReportItem? SavedClosing
    );
}
