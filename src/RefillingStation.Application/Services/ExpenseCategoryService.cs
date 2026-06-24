using Microsoft.Extensions.Caching.Memory;
using RefillingStation.Application.DTOs.ExpenseCategories;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class ExpenseCategoryService : IExpenseCategoryService
    {
        private readonly IExpenseCategoryRepository _repository;
        private readonly IMemoryCache _cache;

        public ExpenseCategoryService(
            IExpenseCategoryRepository repository,
            IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }
        public async Task<List<ExpenseCategoryListItemResponse>> GetAllAsync()
        {
            if (!_cache.TryGetValue("ExpenseCategoryList", out List<ExpenseCategoryListItemResponse>? cachedList) || cachedList is null)
            {
                var expenseCategories = await _repository.GetAllAsync();

                cachedList = expenseCategories
                    .Select(x => new ExpenseCategoryListItemResponse
                    (
                        x.Id,
                        x.Name
                    ))
                    .ToList();

                _cache.Set("ExpenseCategoryList", cachedList, TimeSpan.FromDays(30));
            }

            return cachedList;
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
