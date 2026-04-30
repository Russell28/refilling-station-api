namespace RefillingStation.Api.Features.Expenses.dtos
{
    public class CreateExpenseRequest
    {
        public DateTime Date { get; set; }
        public int ExpenseCategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
