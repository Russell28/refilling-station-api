
namespace RefillingStation.Application.DTOs.Reports
{
    public sealed record ExpenseReportItem(
        int Id,
        DateOnly Date,
        int ExpenseCategoryId,
        string ExpenseCategory,
        decimal Amount
    );
}
