namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record PayrollBreakdownResponse(
        decimal TotalPayrollEarned,
        decimal TotalPayrollPaid,
        decimal PayrollOwed,
        IReadOnlyList<PayrollBreakdownItemResponse> Items
    );
}
