using RefillingStation.Application.DTOs.ExpenseCategories;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IExpenseCategoryService
    {
        Task<List<ExpenseCategoryListItemResponse>> GetAllAsync();
        Task<ExpenseCategoryListItemResponse> GetByIdAsync(int id);
    }
}
