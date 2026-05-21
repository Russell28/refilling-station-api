namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record ExpenseBreakdownResponse(
        decimal TotalExpense,
        IReadOnlyList<ExpenseBreakdownItemResponse> Items
    );
}
