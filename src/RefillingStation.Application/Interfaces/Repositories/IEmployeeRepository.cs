using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(int id);
        Task<List<Employee>> GetAllAsync();
        Task AddAsync(Employee entity);
        Task DeleteAsync(Employee entity);
        Task SaveChangesAsync();
    }
}
