using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface ICustomerDebtRepository
    {
        Task<CustomerDebtEntry?> GetByIdAsync(int id);
        Task<IEnumerable<CustomerDebtEntry>> GetAllAsync();
        Task AddAsync(CustomerDebtEntry entity);
        Task UpdateAsync(CustomerDebtEntry entity);
        Task DeleteAsync(int id);
    }
}
