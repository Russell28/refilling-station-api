namespace RefillingStation.Api.Entities
{
    public class Expense : BaseEntity
    {
        public DateTime Date { get; set; }
        public string ExpenseCategory { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
