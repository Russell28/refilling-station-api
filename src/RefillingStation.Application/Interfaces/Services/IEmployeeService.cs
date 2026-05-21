using RefillingStation.Application.DTOs.Employees;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<List<EmployeeListItemResponse>> GetAllAsync();
        Task<EmployeeListItemResponse> GetByIdAsync(int id);
        Task<List<EmployeeListItemResponse>> GetActiveEmployeesAsync();
    }
}
