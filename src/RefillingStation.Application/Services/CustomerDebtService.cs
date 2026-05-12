using FluentValidation;
using RefillingStation.Application.DTOs.CustomerDebts;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Services
{
    public class CustomerDebtService : ICustomerDebtService
    {
        private readonly ICustomerDebtRepository _customerDebtRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IValidator<CustomerDebtCreateRequest> _validator;

        public CustomerDebtService(
            ICustomerDebtRepository customerDebtRepository,
            ICustomerRepository customerRepository,
            IValidator<CustomerDebtCreateRequest> validator)
        {
            _customerDebtRepository = customerDebtRepository;
            _customerRepository = customerRepository;
            _validator = validator;
        }
        public async Task<List<CustomerDebtResponse>> GetAllAsync()
        {
            var debts = await _customerDebtRepository.GetAllAsync();

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
            var debt = await _customerDebtRepository.GetByIdAsync(id);

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

            var customerExists = await _customerRepository.ExistsAsync(e => e.Id == request.CustomerId);

            if (!customerExists)
                throw new Exception("Customer not found.");

            var debt = new CustomerDebtEntry
            {
                Date = request.Date,
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Notes = request.Notes
            };

            await _customerDebtRepository.AddAsync(debt);
            await _customerDebtRepository.SaveChangesAsync();

            return debt.Id;
        }
        
        public async Task UpdateAsync(int id, CustomerDebtCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var customerExists = await _customerRepository.ExistsAsync(e => e.Id == request.CustomerId);

            if (!customerExists)
                throw new Exception("Customer not found.");

            var debt = await _customerDebtRepository.GetByIdAsync(id);

            if (debt is null)
                throw new Exception("Debt not found.");

            debt.Date = request.Date;
            debt.Amount = request.Amount;
            debt.CustomerId = request.CustomerId;
            debt.Notes = request.Notes;

            await _customerDebtRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var debt = await _customerDebtRepository.GetByIdAsync(id);

            if (debt is null)
                throw new Exception("Debt not found.");

            _customerDebtRepository.Remove(debt);

            await _customerDebtRepository.SaveChangesAsync();
        }

    }
}
