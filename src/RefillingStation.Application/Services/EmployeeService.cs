using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using RefillingStation.Application.DTOs.Employees;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly IValidator<EmployeeCreateRequest> _validator;

        public EmployeeService(
            IEmployeeRepository repository,
            IMemoryCache cache,
            IValidator<EmployeeCreateRequest> validator)
        {
            _repository = repository;
            _cache = cache;
            _validator = validator;
        }

        public async Task<List<EmployeeDetailResponse>> GetAllAsync()
        {
            if (!_cache.TryGetValue("EmployeeList", out List<EmployeeDetailResponse>? cachedList) || cachedList is null)
            {
                var employees = await _repository.GetAllAsync();

                cachedList = employees
                    .Select(x => new EmployeeDetailResponse(
                            x.Id,
                            x.FirstName,
                            x.LastName,
                            x.PhoneNumber,
                            x.Role.ToString(),
                            x.EmploymentType.ToString(),
                            x.IsActive,
                            x.DeactivatedAt
                        )
                    )
                    .ToList();

                _cache.Set("EmployeeList", cachedList, TimeSpan.FromDays(7));
            }

            return cachedList;
        }

        public async Task<EmployeeDetailResponse> GetByIdAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee is null)
                throw new NotFoundException("Employee", id);

            return new EmployeeDetailResponse(
                employee.Id,
                employee.FirstName,
                employee.LastName,
                employee.PhoneNumber,
                employee.Role.ToString(),
                employee.EmploymentType.ToString(),
                employee.IsActive,
                employee.DeactivatedAt
            );
        }

        public async Task<List<EmployeeListItemResponse>> GetActiveEmployeesAsync()
        {
            if (!_cache.TryGetValue("ActiveEmployeeList", out List<EmployeeListItemResponse>? cachedList) || cachedList is null)
            {
                var employees = await _repository.GetActiveAsync();

                cachedList = employees
                    .Select(x => new EmployeeListItemResponse(
                            x.Id,
                            x.FullName
                        )
                    )
                    .ToList();

                _cache.Set("ActiveEmployeeList", cachedList, TimeSpan.FromDays(7));
            }

            return cachedList;
        }

        public async Task<int> CreateAsync(EmployeeCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var employee = new Employee
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                EmploymentType = request.EmploymentType,
            };

            await _repository.AddAsync(employee);
            await _repository.SaveChangesAsync();

            ClearCache();

            return employee.Id;
        }

        public async Task UpdateAsync(int id, EmployeeCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var employee = await _repository.GetByIdAsync(id);

            if (employee is null)
                throw new NotFoundException("Employee", id);

            employee.FirstName = request.FirstName;
            employee.LastName = request.LastName;
            employee.PhoneNumber = request.PhoneNumber;
            employee.Role = request.Role;
            employee.EmploymentType = request.EmploymentType;

            await _repository.SaveChangesAsync();

            ClearCache();
        }

        public async Task DeleteAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee is null)
                throw new NotFoundException("Employee", id);

            _repository.Remove(employee);

            await _repository.SaveChangesAsync();

            ClearCache();
        }

        public async Task ActivateAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee is null)
                throw new NotFoundException("Employee", id);

            employee.Activate();

            await _repository.SaveChangesAsync();

            ClearCache();
        }

        public async Task DeactivateAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee is null)
                throw new NotFoundException("Employee", id);

            employee.Deactivate();

            await _repository.SaveChangesAsync();

            ClearCache();
        }

        private void ClearCache()
        {
            // Invalidate cache so next call reloads fresh
            _cache.Remove("EmployeeList");
            _cache.Remove("ActiveEmployeeList");
        }
    }
}
