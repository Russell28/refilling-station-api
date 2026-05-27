using FluentValidation;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.CustomerDebts;
using RefillingStation.Application.DTOs.Expenses;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IExpenseCategoryRepository _expenseCategoryRepository;
        private readonly IValidator<ExpenseCreateRequest> _validator;

        public ExpenseService(
            IExpenseRepository expenseRepository,
            IExpenseCategoryRepository expenseCategoryRepository,
            IValidator<ExpenseCreateRequest> validator)
        {
            _expenseRepository = expenseRepository;
            _expenseCategoryRepository = expenseCategoryRepository;
            _validator = validator;
        }
        public async Task<List<ExpenseDetailResponse>> GetAllAsync()
        {
            var expenses = await _expenseRepository.GetAllAsync();

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
            var expense = await _expenseRepository.GetByIdAsync(id);

            if (expense is null)
                throw new NotFoundException("Expense", id);

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

            var expenseCategoryExists = await _expenseCategoryRepository.ExistsAsync(e => e.Id == request.ExpenseCategoryId);

            if (!expenseCategoryExists)
                throw new NotFoundException("Category", request.ExpenseCategoryId);

            var expense = new Expense
            {
                Date = request.Date,
                ExpenseCategoryId = request.ExpenseCategoryId,
                Amount = request.Amount,
                Notes = request.Notes
            };

            await _expenseRepository.AddAsync(expense);
            await _expenseRepository.SaveChangesAsync();

            return expense.Id;
        }

        public async Task UpdateAsync(int id, ExpenseCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var expenseCategoryExists = await _expenseCategoryRepository.ExistsAsync(e => e.Id == request.ExpenseCategoryId);

            if (!expenseCategoryExists)
                throw new NotFoundException("Category", request.ExpenseCategoryId);

            var expense = await _expenseRepository.GetByIdAsync(id);

            if (expense is null)
                throw new NotFoundException("Expense", id);

            expense.Date = request.Date;
            expense.ExpenseCategoryId = request.ExpenseCategoryId;
            expense.Amount = request.Amount;
            expense.Notes = request.Notes;

            await _expenseRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);

            if (expense is null)
                throw new NotFoundException("Expense", id);

            _expenseRepository.Remove(expense);

            await _expenseRepository.SaveChangesAsync();
        }

        public async Task<List<ExpenseDetailResponse>> SearchByDateRangeAsync(DateRangeRequest request)
        {
            var options = new DateRangeOptions
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Page = request.Page
            };

            var expenses = await _expenseRepository.SearchByDateRangeAsync(options);

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
    }
}
