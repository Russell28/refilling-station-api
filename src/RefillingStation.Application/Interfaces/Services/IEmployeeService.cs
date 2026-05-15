using RefillingStation.Application.DTOs.Employees;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<List<EmployeeListItemResponse>> GetAllAsync();
        Task<EmployeeListItemResponse> GetByIdAsync(int id);
        Task<List<EmployeeListItemResponse>> GetActiveEmployeesAsync();



        //Task<int> CreateAsync(EmployeeCreateRequest request);
        //Task UpdateAsync(int id, EmployeeCreateRequest request);
        //Task DeleteAsync(int id);
    }
}
