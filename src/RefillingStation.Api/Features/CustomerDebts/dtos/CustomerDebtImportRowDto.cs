using CsvHelper.Configuration.Attributes;

namespace RefillingStation.Api.Features.CustomerDebts.dtos
{
    public class CustomerDebtImportRowDto
    {
        public string Date { get; set; } = string.Empty;
        [Name("Customer Name")]
        public string CustomerName { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
