using FluentValidation;
using RefillingStation.Application.DTOs.Expenses;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _repository;
        private readonly IValidator<ExpenseCreateRequest> _validator;

        public ExpenseService(
            IExpenseRepository repository,
            IValidator<ExpenseCreateRequest> validator)
        {
            _repository = repository;
            _validator = validator;
        }
        public async Task<List<ExpenseDetailResponse>> GetAllAsync()
        {
            var expenses = await _repository.GetAllAsync();

            return expenses
                .Select(x => new ExpenseDetailResponse
                (
                    x.Id,
                    x.Date,
                    x.ExpenseCategoryId,
                    x.Category.Name,
                    x.Amount,
                    x.Notes
                ))
                .ToList();
        }

        public async Task<ExpenseDetailResponse> GetByIdAsync(int id)
        {
            var expense = await _repository.GetByIdAsync(id);

            if (expense is null)
                throw new Exception("Expense not found.");

            return new ExpenseDetailResponse(
                expense.Id,
                expense.Date,
                expense.ExpenseCategoryId,
                expense.Category.Name,
                expense.Amount,
                expense.Notes
            );
        }

        public async Task<int> CreateAsync(ExpenseCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var expense = new Expense
            {
                Date = request.Date,
                ExpenseCategoryId = request.ExpenseCategoryId,
                Amount = request.Amount,
                Notes = request.Notes
            };

            await _repository.AddAsync(expense);
            await _repository.SaveChangesAsync();

            return expense.Id;
        }

        public async Task UpdateAsync(int id, ExpenseCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var expense = await _repository.GetByIdAsync(id);

            if (expense is null)
                throw new Exception("Expense not found.");

            expense.Date = request.Date;
            expense.ExpenseCategoryId = request.ExpenseCategoryId;
            expense.Amount = request.Amount;
            expense.Notes = request.Notes;

            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var expense = await _repository.GetByIdAsync(id);

            if (expense is null)
                throw new Exception("Expense not found.");

            _repository.Remove(expense);

            await _repository.SaveChangesAsync();
        }
    }
}
