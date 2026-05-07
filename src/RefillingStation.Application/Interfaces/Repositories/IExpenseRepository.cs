using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IExpenseRepository
    {
        Task<Expense?> GetByIdAsync(int id);
        Task<IEnumerable<Expense>> GetAllAsync();
        Task AddAsync(Expense entity);
        Task UpdateAsync(Expense entity);
        Task DeleteAsync(int id);
    }
}
