namespace RefillingStation.Application.DTOs.CustomerDebts
{
    public class CreateDebtRequest
    {
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
