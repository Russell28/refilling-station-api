using RefillingStation.Application.DTOs.Reports.Breakdowns;

namespace RefillingStation.Application.DTOs.Reports.DailySummary
{
    public sealed record DailySummaryAdminResponse(
        DailySummaryInfoAdminResponse Summary,
        ExpenseBreakdownResponse ExpenseBreakdown,
        DebtBreakdownResponse DebtBreakdown,
        PayrollBreakdownResponse PayrollBreakdown
    );
}
