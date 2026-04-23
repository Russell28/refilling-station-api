namespace RefillingStation.Api.Entities
{
    public class CustomerDebtEntry : BaseEntity
    {
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        // Positive = debt created, Negative = payment received
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public Customer Customer { get; set; } = null!;
    }
}
