using RefillingStation.Application.DTOs.ExpenseCategories;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class ExpenseCategoryService : IExpenseCategoryService
    {
        private readonly IExpenseCategoryRepository _repository;

        public ExpenseCategoryService(
            IExpenseCategoryRepository repository)
        {
            _repository = repository;
        }
        public async Task<List<ExpenseCategoryListItemResponse>> GetAllAsync()
        {
            var expenseCategorys = await _repository.GetAllAsync();

            return expenseCategorys
                .Select(x => new ExpenseCategoryListItemResponse
                (
                    x.Id,
                    x.Name
                ))
                .ToList();
        }

        public async Task<ExpenseCategoryListItemResponse> GetByIdAsync(int id)
        {
            var expenseCategory = await _repository.GetByIdAsync(id);

            if (expenseCategory is null)
                throw new NotFoundException("ExpenseCategory", id);

            return new ExpenseCategoryListItemResponse(
                expenseCategory.Id,
                expenseCategory.Name
            );
        }
    }
}
