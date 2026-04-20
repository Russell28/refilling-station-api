using CsvHelper.Configuration.Attributes;

namespace RefillingStation.Api.Features.Payrolls.dtos
{
    public class PayrollEntryImportRowDto
    {
        [Name("Earned Date")]
        public string EarnedDate { get; set; } = string.Empty;
        [Name("Paid Date")]
        public string PaidDate { get; set; } = string.Empty;
        [Name("Employee Name")]
        public string EmployeeName { get; set; } = string.Empty;
        [Name("Salary Amount")]
        public string SalaryAmount { get; set; } = string.Empty;
        [Name("Cash Paid")]
        public string CashPaid { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
