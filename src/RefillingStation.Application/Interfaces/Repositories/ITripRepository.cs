using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface ITripRepository
    {
        Task<Trip?> GetByIdAsync(int id);
        Task<List<Trip>> GetAllAsync();
        Task AddAsync(Trip entity);
        Task DeleteAsync(Trip entity);
        Task SaveChangesAsync();
    }
}
