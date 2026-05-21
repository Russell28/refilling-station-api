using CsvHelper.Configuration;
using RefillingStation.Application.DTOs.Imports;
namespace RefillingStation.Infrastructure.Imports.Shared.Mappings
{
    public sealed class ExpenseImportRowRequestMap : ClassMap<ExpenseImportRowRequest>
    {
        public ExpenseImportRowRequestMap()
        {
            Map(m => m.Date)
            .Name("Date");

            Map(m => m.ExpenseCategory)
                .Name("Category");

            Map(m => m.Amount)
                .Name("Amount");

            Map(m => m.Notes)
                .Name("Notes");
        }
    }
}
