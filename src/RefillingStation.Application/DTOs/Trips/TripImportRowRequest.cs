namespace RefillingStation.Application.DTOs.Trips
{
    public class TripImportRowRequest
    {
        public string Date { get; set; } = string.Empty;
        public string TripNo { get; set; } = string.Empty;
        public string TimeStarted { get; set; } = string.Empty;
        public string TimeEnded { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string TripType { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string CustomerCategory { get; set; } = string.Empty;
        public string CollectedQty { get; set; } = string.Empty;
        public string LoadedQty { get; set; } = string.Empty;
        public string DeliveredQty { get; set; } = string.Empty;
        public string ReturnedQty { get; set; } = string.Empty;
        public string ReplacementQty { get; set; } = string.Empty;
        public string FreeQty { get; set; } = string.Empty;

        public string ActualCashCollected { get; set; } = string.Empty;
        public string IsRemitted { get; set; } = string.Empty;
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
