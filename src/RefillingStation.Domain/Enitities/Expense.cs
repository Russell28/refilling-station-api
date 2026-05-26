namespace RefillingStation.Domain.Entities
{
    public class Expense : BaseEntity
    {
        public DateOnly Date { get; set; }
        public int ExpenseCategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }

        // Relationships
        public ExpenseCategory Category { get; set; } = null!;
    }
}
