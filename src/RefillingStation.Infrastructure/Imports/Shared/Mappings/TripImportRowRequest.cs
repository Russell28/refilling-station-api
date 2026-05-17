using CsvHelper.Configuration;
using RefillingStation.Application.DTOs.Imports;
namespace RefillingStation.Infrastructure.Imports.Shared.Mappings
{
    public sealed class TripImportRowRequestMap : ClassMap<TripImportRowRequest>
    {
        public TripImportRowRequestMap()
        {
            Map(m => m.Date)
                .Name("Date");

            Map(m => m.TripNo)
                .Name("Trip No");

            Map(m => m.TimeStarted)
                .Name("Time Started");

            Map(m => m.TimeEnded)
                .Name("Time Ended");

            Map(m => m.Source)
                .Name("Source");

            Map(m => m.TripType)
                .Name("Trip Type");

            Map(m => m.EmployeeName)
                .Name("Employee Name");

            Map(m => m.CustomerCategory)
                .Name("Customer Category");

            Map(m => m.CollectedQty)
                .Name("Collected Qty");

            Map(m => m.LoadedQty)
                .Name("Loaded Qty");

            Map(m => m.DeliveredQty)
                .Name("Delivered Qty");

            Map(m => m.ReturnedQty)
                .Name("Returned Qty");

            Map(m => m.ReplacementQty)
                .Name("Replacement Qty");

            // CSV has this column but your DTO does not
            // We ignore it to avoid errors
            Map(m => m.FreeQty)
                .Name("Free Qty");

            Map(m => m.ActualCashCollected)
                .Name("Actual Cash Collected");

            Map(m => m.IsRemitted)
                .Name("Is Remitted");

            Map(m => m.Notes)
                .Name("Notes");
        }

    }
}
