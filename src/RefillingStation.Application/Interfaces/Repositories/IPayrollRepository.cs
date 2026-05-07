using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IPayrollRepository
    {
        Task<PayrollEntry?> GetByIdAsync(int id);
        Task<IEnumerable<PayrollEntry>> GetAllAsync();
        Task AddAsync(PayrollEntry entity);
        Task UpdateAsync(PayrollEntry entity);
        Task DeleteAsync(int id);
    }
}
