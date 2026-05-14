
namespace RefillingStation.Application.DTOs.Reports
{
    public sealed record ExpenseReportItem(
        int Id,
        DateTime Date,
        int ExpenseCategoryId,
        string ExpenseCategory,
        decimal Amount
    );
}
