namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record PayrollBreakdownResponse(
        decimal TotalEarned,
        decimal TotalPaid,
        decimal TotalOwed,
        IReadOnlyList<PayrollBreakdownItemResponse> Items
    );
}
