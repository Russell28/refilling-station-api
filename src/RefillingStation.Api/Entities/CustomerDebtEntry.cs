namespace RefillingStation.Api.Entities
{
    public class CustomerDebtEntry : BaseEntity
    {
        public DateTime Date { get; set; }
        public string? CustomerName { get; set; }
        // Positive = debt created, Negative = payment received
        public decimal Amount { get; set; }
        public int? RelatedTripId { get; set; }
        public string? EntryType { get; set; }
        public string? Notes { get; set; }
    }
}
