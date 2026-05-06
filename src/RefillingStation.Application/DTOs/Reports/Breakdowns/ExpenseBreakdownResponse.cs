namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record ExpenseBreakdownResponse(
        decimal TotalExpenses,
        IReadOnlyList<ExpenseBreakdownItemResponse> Items
    );
}
