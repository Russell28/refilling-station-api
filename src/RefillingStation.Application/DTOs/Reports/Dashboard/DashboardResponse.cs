using RefillingStation.Application.DTOs.Reports.Breakdowns;

namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed record DashboardResponse(
        DashboardSummaryResponse Summary,
        IReadOnlyList<DailyReportItem> DailyReports,
        ExpenseBreakdownResponse ExpenseBreakdown,
        DebtBreakdownResponse DebtBreakdown,
        PayrollBreakdownResponse PayrollBreakdown
    );
}
