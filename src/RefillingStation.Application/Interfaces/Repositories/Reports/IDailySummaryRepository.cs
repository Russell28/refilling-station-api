using RefillingStation.Application.DTOs.Reports.DailySummary;

namespace RefillingStation.Application.Interfaces.Repositories.Reports
{
    public interface IDailySummaryRepository
    {
        Task<DailySummaryRawData> GetDailySummaryAsync(DateTime date);
    }
}
