using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IExpenseCategoryRepository
    {
        Task<ExpenseCategory?> GetByIdAsync(int id);
        Task<IEnumerable<ExpenseCategory>> GetAllAsync();
        Task AddAsync(ExpenseCategory entity);
        Task UpdateAsync(ExpenseCategory entity);
        Task DeleteAsync(int id);
    }
}
