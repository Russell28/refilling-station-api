using System.ComponentModel.DataAnnotations.Schema;

namespace RefillingStation.Api.Entities
{
    public class Trip : BaseEntity
    {
        public DateOnly Date { get; set; }
        public int TripNumber { get; set; }
        public DateTime? TimeStarted { get; set; }
        public DateTime? TimeEnded { get; set; }

        // ===== Source info =====
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? Source { get; set; }
        public string? TripType { get; set; }
        public string? CustomerCategory { get; set; }

        // ===== Quantities =====
        public decimal CollectedQty { get; set; }
        public decimal LoadedQty { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal FreeQty { get; set; }
        public decimal ReturnedQty { get; set; }
        public decimal ReplacementQty { get; set; }

        // ===== Cash =====
        public decimal ActualCashCollected { get; set; }
        public bool IsRemitted { get; set; } = false;

        // ===== Notes =====
        public string? Notes { get; set; }

        // Relationships
        public Employee Employee { get; set; } = null!;
    }
}
