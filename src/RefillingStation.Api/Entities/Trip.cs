using System.ComponentModel.DataAnnotations.Schema;

namespace RefillingStation.Api.Entities
{
    public class Trip : BaseEntity
    {
        public DateTime Date { get; set; }
        public int TripNumber { get; set; }
        public string Segment { get; set; } = string.Empty;

        public DateTime? TimeStarted { get; set; }
        public DateTime? TimeEnded { get; set; }

        // ===== Source info =====
        public string Source { get; set; } = string.Empty;
        public string TripType { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string CustomerCategory { get; set; } = string.Empty;

        // ===== Quantities =====
        public decimal CollectedQty { get; set; }
        public decimal LoadedQty { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal ActualPaidQty { get; set; }
        public decimal FreeQty { get; set; }
        public decimal ReturnedQty { get; set; }
        public decimal ReplacementQty { get; set; }

        // ===== Cash =====
        public decimal ActualCashCollected { get; set; }
        public bool IsRemitted { get; set; } = false;

        // ===== Notes =====
        public int? RelatedTripId { get; set; }
        public string Notes { get; set; } = string.Empty;

        //public decimal? EstimatedPricePerGallon { get; set; }
        //[NotMapped]
        //public decimal? ToBePaidQty => LoadedQty - FreeQty - ReturnedQty;
        //[NotMapped]
        //public decimal? EstimatedCash => ToBePaidQty * EstimatedPricePerGallon;

        
       
    }
}
