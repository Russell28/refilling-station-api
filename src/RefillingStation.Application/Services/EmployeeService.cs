using RefillingStation.Application.DTOs.Employees;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeService(
            IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<EmployeeListItemResponse>> GetAllAsync()
        {
            var employees = await _repository.GetAllAsync();

            return employees
                .Select(x => new EmployeeListItemResponse
                (
                    x.Id,
                    x.FullName
                ))
                .ToList();
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
