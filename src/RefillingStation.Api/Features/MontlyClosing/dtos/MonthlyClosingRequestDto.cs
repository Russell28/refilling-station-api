namespace RefillingStation.Api.Features.MontlyClosing.dtos
{
    public class MonthlyClosingRequestDto
    {
        public string Month { get; set; } = String.Empty;
        public decimal ManagerShare { get; set; }
        public decimal OwnerShare { get; set; }
        public string? Notes{ get; set; }
    }
}
