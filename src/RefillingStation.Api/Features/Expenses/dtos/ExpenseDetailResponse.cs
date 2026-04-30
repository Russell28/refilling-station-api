namespace RefillingStation.Api.Features.Expenses.dtos
{
    public record ExpenseDetailResponse(
        int Id,
        DateTime Date,
        int ExpenseCategoryId,
        string ExpenseCategory,
        decimal Amount,
        string? Notes
    );
}
