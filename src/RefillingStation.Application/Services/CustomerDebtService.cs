using FluentValidation;
using RefillingStation.Application.DTOs.CustomerDebts;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Services
{
    public class CustomerDebtService : ICustomerDebtService
    {
        private readonly ICustomerDebtRepository _repository;
        private readonly IValidator<CustomerDebtCreateRequest> _validator;

        public CustomerDebtService(
            ICustomerDebtRepository repository,
            IValidator<CustomerDebtCreateRequest> validator)
        {
            _repository = repository;
            _validator = validator;
        }
        public async Task<List<CustomerDebtResponse>> GetAllAsync()
        {
            var debts = await _repository.GetAllAsync();

            return debts
                .Select(x => new CustomerDebtResponse
                (
                    x.Id,
                    x.Date,
                    x.CustomerId,
                    x.Customer.Name,
                    x.Amount,
                    x.Notes
                ))
                .ToList();
        }

        public async Task<CustomerDebtResponse> GetByIdAsync(int id)
        {
            var debt = await _repository.GetByIdAsync(id);

            if (debt is null)
                throw new Exception("Debt not found.");

            return new CustomerDebtResponse(
                debt.Id,
                debt.Date,
                debt.CustomerId,
                debt.Customer.Name,
                debt.Amount,
                debt.Notes
            );
        }

        public async Task<int> CreateAsync(CustomerDebtCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var debt = new CustomerDebtEntry
            {
                Date = request.Date,
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Notes = request.Notes
            };

            await _repository.AddAsync(debt);
            await _repository.SaveChangesAsync();

            return debt.Id;
        }
        
        public async Task UpdateAsync(int id, CustomerDebtCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var debt = await _repository.GetByIdAsync(id);

            if (debt is null)
                throw new Exception("Debt not found.");

            debt.Date = request.Date;
            debt.Amount = request.Amount;
            debt.CustomerId = request.CustomerId;
            debt.Notes = request.Notes;

            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var debt = await _repository.GetByIdAsync(id);

            if (debt is null)
                throw new Exception("Debt not found.");

            _repository.Remove(debt);

            await _repository.SaveChangesAsync();
        }

    }
}
