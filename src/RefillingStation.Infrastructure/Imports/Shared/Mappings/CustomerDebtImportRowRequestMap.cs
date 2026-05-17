using CsvHelper.Configuration;
using RefillingStation.Application.DTOs.Imports;
namespace RefillingStation.Infrastructure.Imports.Shared.Mappings
{
    public sealed class CustomerDebtImportRowRequestMap : ClassMap<CustomerDebtImportRowRequest>
    {
        public CustomerDebtImportRowRequestMap()
        {
            Map(m => m.Date)
            .Name("Date");

            Map(m => m.CustomerName)
                .Name("Customer Name");

            Map(m => m.Amount)
                .Name("Amount");

            Map(m => m.Notes)
                .Name("Notes");
        }
    }
}
