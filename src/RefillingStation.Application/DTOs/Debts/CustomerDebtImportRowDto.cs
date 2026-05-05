namespace RefillingStation.Application.DTOs.Debts
{
    public class CustomerDebtImportRowDto
    {
        public string Date { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
