namespace RefillingStation.Api.Features.Expenses.dtos
{
    public class CreateExpenseRequest
    {
        public DateTime Date { get; set; }
        public string ExpenseCategory { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
