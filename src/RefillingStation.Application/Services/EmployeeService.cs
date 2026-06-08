using Microsoft.Extensions.Caching.Memory;
using RefillingStation.Application.DTOs.Employees;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IMemoryCache _cache;

        public EmployeeService(
            IEmployeeRepository repository,
            IMemoryCache cache)
        {
            _cache = cache;
            _repository = repository;
        }

        public async Task<List<EmployeeListItemResponse>> GetAllAsync()
        {
            if (!_cache.TryGetValue("EmployeeList", out List<EmployeeListItemResponse>? cachedList) || cachedList is null)
            {
                var employees = await _repository.GetAllAsync();

                cachedList = employees
                    .Select(x => new EmployeeListItemResponse(x.Id, x.FullName))
                    .ToList();

                _cache.Set("EmployeeList", cachedList, TimeSpan.FromDays(7));
            }

            return cachedList;
        }

        public async Task<EmployeeListItemResponse> GetByIdAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee is null)
                throw new NotFoundException("Employee", id);

            return new EmployeeListItemResponse(
                employee.Id,
                employee.FullName
            );
        }

        public async Task<List<EmployeeListItemResponse>> GetActiveEmployeesAsync()
        {
            var employees = await _repository.GetAllActiveAsync();

            return employees
                .Select(x => new EmployeeListItemResponse
                (
                    x.Id,
                    x.FullName
                ))
                .ToList();
        }
    }
}
