using RefillingStation.Application.DTOs.Expenses;

namespace RefillingStation.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDetailResponse>> GetAllAsync();
        Task<ExpenseDetailResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(ExpenseCreateRequest request);
        Task UpdateAsync(int id, ExpenseCreateRequest request);
        Task DeleteAsync(int id);
    }
}
