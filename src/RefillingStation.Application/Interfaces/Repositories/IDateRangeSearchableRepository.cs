using RefillingStation.Application.DTOs.Common;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IDateRangeSearchableRepository<T> where T : class
    {
        Task<List<T>> SearchByDateRangeAsync(DateRangeOptions options);
    }
}
