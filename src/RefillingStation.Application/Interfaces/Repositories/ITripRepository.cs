using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface ITripRepository : IBaseRepository<Trip>, IDateRangeSearchableRepository<Trip>
    {
        Task<int?> GetMaxTripNumberByDateAsync(DateOnly date);
    }
}
