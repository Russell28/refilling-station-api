using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.Expenses;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IExpenseService
    {
        Task<List<ExpenseDetailResponse>> GetAllAsync();
        Task<ExpenseDetailResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(ExpenseCreateRequest request);
        Task UpdateAsync(int id, ExpenseCreateRequest request);
        Task DeleteAsync(int id);
        Task<List<ExpenseDetailResponse>> SearchByDateRangeAsync(DateRangeRequest request);

    }
}
