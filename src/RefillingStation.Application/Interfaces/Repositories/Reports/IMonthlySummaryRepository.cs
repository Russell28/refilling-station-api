using RefillingStation.Application.DTOs.Reports.MonthlySummary;

namespace RefillingStation.Application.Interfaces.Repositories.Reports
{
    public interface IMonthlySummaryRepository
    {
        Task<MonthlySummaryRawData> GetMonthlySummaryAsync(DateOnly start, DateOnly end);
    }
}
