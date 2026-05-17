using CsvHelper.Configuration;
using RefillingStation.Application.DTOs.Imports;
namespace RefillingStation.Infrastructure.Imports.Shared.Mappings
{
    public sealed class PayrollImportRowRequestMap : ClassMap<PayrollImportRowRequest>
    {
        public PayrollImportRowRequestMap()
        {
            Map(m => m.EarnedDate)
                .Name("Earned Date");

            Map(m => m.PaidDate)
                .Name("Paid Date");

            Map(m => m.EmployeeName)
                .Name("Employee Name");

            Map(m => m.SalaryAmount)
                .Name("Salary Amount");

            Map(m => m.CashPaid)
                .Name("Cash Paid");

            Map(m => m.Notes)
                .Name("Notes");
        }
    }
}
