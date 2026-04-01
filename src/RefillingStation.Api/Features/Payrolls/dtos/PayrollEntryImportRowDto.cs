using CsvHelper.Configuration.Attributes;

namespace RefillingStation.Api.Features.Payrolls.dtos
{
    public class PayrollEntryImportRowDto
    {
        public string Date { get; set; } = string.Empty;
        [Name("Employee Name")]
        public string EmployeeName { get; set; } = string.Empty;
        [Name("Salary Amount")]
        public string SalaryAmount { get; set; } = string.Empty;
        [Name("Advance Given")]
        public string AdvanceGiven { get; set; } = string.Empty;
        [Name("Advance Deduction")]
        public string AdvanceDeduction { get; set; } = string.Empty;
        [Name("Cash Paid")]
        public string CashPaid { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
