using CsvHelper.Configuration.Attributes;

namespace RefillingStation.Api.Features.Expenses.dtos
{
    public class ExpenseImportRowDto
    {
        public string Date { get; set; }
        [Name("Category")]
        public string ExpenseCategory { get; set; } = string.Empty;
        public string Amount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
