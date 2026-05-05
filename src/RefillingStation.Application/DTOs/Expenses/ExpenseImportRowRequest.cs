namespace RefillingStation.Application.DTOs.Expenses
{
    public class ExpenseImportRowRequest
    {
        public string Date { get; set; } = string.Empty;
        public string ExpenseCategory { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
