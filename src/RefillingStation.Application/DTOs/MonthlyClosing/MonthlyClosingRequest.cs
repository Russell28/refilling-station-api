namespace RefillingStation.Application.DTOs.MonthlyClosing
{
    public class MonthlyClosingRequest
    {
        public string MonthYear { get; set; } = string.Empty;
        public decimal ManagerShare { get; set; }
        public decimal OwnerShare { get; set; }
        public string? Notes{ get; set; }
    }
}
