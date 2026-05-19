namespace RefillingStation.Application.DTOs.Imports
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
}
