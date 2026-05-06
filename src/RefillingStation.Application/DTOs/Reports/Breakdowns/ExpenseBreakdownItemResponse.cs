namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record ExpenseBreakdownItemResponse(
        string Category,
        decimal Amount
    );
}
