
using FluentValidation;
using RefillingStation.Application.DTOs.Customers;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly IValidator<CustomerCreateRequest> _validator;

        public CustomerService(
            ICustomerRepository repository,
            IValidator<CustomerCreateRequest> validator)
        {
            _repository = repository;
            _validator = validator;
        }
        public async Task<List<CustomerListItemResponse>> GetAllAsync()
        {
            var customers = await _repository.GetAllAsync();

            return customers
                .Select(x => new CustomerListItemResponse
                (
                    x.Id,
                    x.Name
                ))
                .ToList();
        }

        public async Task<CustomerListItemResponse> GetByIdAsync(int id)
        {
            var customer = await _repository.GetByIdAsync(id);

            if (customer is null)
                throw new NotFoundException("Customer", id);

            return new CustomerListItemResponse(
                customer.Id,
                customer.Name
            );
        }

        public async Task<int> CreateAsync(CustomerCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var customer = new Customer
            {
                Name = request.Name,
            };

            await _repository.AddAsync(customer);
            await _repository.SaveChangesAsync();

            return customer.Id;
        }

        public async Task UpdateAsync(int id, CustomerCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var customer = await _repository.GetByIdAsync(id);

            if (customer is null)
                throw new NotFoundException("Customer", id);

            customer.Name = request.Name;

            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _repository.GetByIdAsync(id);

            if (customer is null)
                throw new NotFoundException("Customer", id);

            _repository.Remove(customer);

            await _repository.SaveChangesAsync();
        }
    }
}
