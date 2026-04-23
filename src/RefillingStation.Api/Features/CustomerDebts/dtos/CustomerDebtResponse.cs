namespace RefillingStation.Api.Features.CustomerDebts.dtos
{
    public class CustomerDebtResponse
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
