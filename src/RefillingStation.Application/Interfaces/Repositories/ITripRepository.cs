using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface ITripRepository
    {
        Task<Trip?> GetByIdAsync(int id);
        Task<IEnumerable<Trip>> GetAllAsync();
        Task AddAsync(Trip entity);
        Task UpdateAsync(Trip entity);
        Task DeleteAsync(int id);
    }
}
