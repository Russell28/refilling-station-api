using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface ITripRepository : IBaseRepository<Trip>
    {
        Task<int?> GetMaxTripNumberByDateAsync(DateOnly date);
    }
}
