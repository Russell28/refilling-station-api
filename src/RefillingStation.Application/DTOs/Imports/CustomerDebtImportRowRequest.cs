namespace RefillingStation.Application.DTOs.Imports
{
    public class CustomerDebtImportRowRequest
    {
        public string Date { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
