using CsvHelper.Configuration.Attributes;

namespace RefillingStation.Api.Features.Trips
{
    public class TripImportRowDto
    {
        public string Date { get; set; } = string.Empty;

        [Name("Trip No")]
        public string TripNo { get; set; } = string.Empty;

        public string Segment { get; set; } = string.Empty;

        [Name("Time Started")]
        public string TimeStarted { get; set; } = string.Empty;

        [Name("Time Ended")]
        public string TimeEnded { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        [Name("Trip Type")]
        public string TripType { get; set; } = string.Empty;

        public string Employee { get; set; } = string.Empty;

        [Name("Customer Category")]
        public string CustomerCategory { get; set; } = string.Empty;

        [Name("Collected Qty")]
        public string CollectedQty { get; set; } = string.Empty;

        [Name("Loaded Qty")]
        public string LoadedQty { get; set; } = string.Empty;

        [Name("Delivered Qty")]
        public string DeliveredQty { get; set; } = string.Empty;

        [Name("Actual Paid Qty")]
        public string ActualPaidQty { get; set; } = string.Empty;

        [Name("Returned Qty")]
        public string ReturnedQty { get; set; } = string.Empty;

        [Name("Replacement Qty")]
        public string ReplacementQty { get; set; } = string.Empty;

        [Name("Free Qty")]
        public string FreeQty { get; set; } = string.Empty;

        [Name("Actual Cash Collected")]
        public string ActualCashCollected { get; set; } = string.Empty;

        [Name("Is Remitted")]
        public string IsRemitted { get; set; } = string.Empty;

        [Name("Related Trip Id")]
        public string RelatedTripId { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }

    public class TripImportError
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class TripImportResult
    {
        public int TotalRows { get; set; }
        public int InsertedRows { get; set; }
        public int FailedRows { get; set; }
        public List<TripImportError> Errors { get; set; } = new();
    }
}
