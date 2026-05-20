namespace RefillingStation.Application.DTOs.Expenses
{
    public sealed record ExpenseDetailResponse(
        int Id,
        DateTime Date,
        int ExpenseCategoryId,
        string ExpenseCategory,
        decimal Amount,
        string? Notes
    );
}
