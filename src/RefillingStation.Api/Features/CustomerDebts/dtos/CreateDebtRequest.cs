namespace RefillingStation.Api.Features.CustomerDebts.dtos
{
    public class CreateDebtRequest
    {
        public DateTime Date { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
