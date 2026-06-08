using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;
        private readonly IValidator<CustomerCreateRequest> _validator;

        public CustomerService(
            ICustomerRepository repository,
            IMemoryCache cache,
            IValidator<CustomerCreateRequest> validator)
        {
            _repository = repository;
            _cache = cache;
            _validator = validator;
        }
        public async Task<List<CustomerListItemResponse>> GetAllAsync()
        {
            if (!_cache.TryGetValue("CustomerList", out List<CustomerListItemResponse>? cachedList) || cachedList is null)
            {
                var customers = await _repository.GetAllAsync();

                cachedList = customers
                    .Select(x => new CustomerListItemResponse(x.Id, x.Name))
                    .ToList();

                _cache.Set("CustomerList", cachedList, TimeSpan.FromDays(7));
            }

            return cachedList;
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

            // Invalidate cache so next call reloads fresh
            _cache.Remove("CustomerList");

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

            // Invalidate cache so next call reloads fresh
            _cache.Remove("CustomerList");
        }
    }
}
