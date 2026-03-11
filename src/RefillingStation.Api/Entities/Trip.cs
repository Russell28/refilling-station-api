namespace RefillingStation.Api.Entities
{
    public class Trip : BaseEntity
    {
        public DateTime Date { get; set; }
        public int TripNumber { get; set; }

        public string Source { get; set; } = string.Empty;
        public string TripType { get; set; } = string.Empty;
        public string Employee { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;

        public decimal CollectedQty { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal FreeQty { get; set; }
        public decimal ToBePaidQty { get; set; }

        public decimal PricePerGallon { get; set; }
        public decimal EstimatedCash { get; set; }
        public decimal PaidQtyActual { get; set; }
        public decimal CashCollectedActual { get; set; }
        public decimal CashUsed { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}
