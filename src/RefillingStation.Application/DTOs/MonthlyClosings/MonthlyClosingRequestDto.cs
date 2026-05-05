namespace RefillingStation.Application.DTOs.MonthlyClosings
{
    public class MonthlyClosingRequestDto
    {
        public string Month { get; set; } = String.Empty;
        public decimal ManagerShare { get; set; }
        public decimal OwnerShare { get; set; }
        public string? Notes{ get; set; }
    }
}
