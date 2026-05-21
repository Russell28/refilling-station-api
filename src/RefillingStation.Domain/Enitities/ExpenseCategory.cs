namespace RefillingStation.Domain.Entities
{
    public class ExpenseCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string? Description { get; set; }

        // Relationships
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
