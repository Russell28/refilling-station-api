namespace RefillingStation.Application.DTOs.Expenses
{
    public class CreateExpenseRequest
    {
        public DateTime Date { get; set; }
        public int ExpenseCategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
