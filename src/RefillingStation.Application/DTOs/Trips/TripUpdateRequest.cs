namespace RefillingStation.Application.DTOs.Trips
{
    public class TripUpdateRequest
    {
        public DateTime Date { get; set; }
        public int TripNumber { get; set; }
        public DateTime? TimeStarted { get; set; }
        public DateTime? TimeEnded { get; set; }

        public int EmployeeId { get; set; }
        public string? Source { get; set; }
        public string? TripType { get; set; }
        public string? CustomerCategory { get; set; }

        public decimal CollectedQty { get; set; }
        public decimal LoadedQty { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal FreeQty { get; set; }
        public decimal ReturnedQty { get; set; }
        public decimal ReplacementQty { get; set; }

        public decimal ActualCashCollected { get; set; }
        public bool IsRemitted { get; set; } = false;

        public string? Notes { get; set; }
    }
}
