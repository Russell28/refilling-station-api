namespace RefillingStation.Api.Entities
{
    public class Expense : BaseEntity
    {
        public DateTime Date { get; set; }
        public int ExpenseCategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }

        // Relationships
        public ExpenseCategory Category { get; set; } = null!;
    }
}
