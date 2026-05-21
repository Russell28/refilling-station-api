namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record ExpenseBreakdownItemResponse(
        int ExpenseCategoryId,
        string CategoryName,
        decimal Amount
    );
}
