using RefillingStation.Application.DTOs.Reports.Breakdowns;

namespace RefillingStation.Application.DTOs.Reports.DailySummary
{
    public sealed record DailySummaryResponse(
        DailySummaryInfoResponse Summary,
        ExpenseBreakdownResponse ExpenseBreakdown,
        DebtBreakdownResponse DebtBreakdown,
        PayrollBreakdownResponse PayrollBreakdown
    );
}
