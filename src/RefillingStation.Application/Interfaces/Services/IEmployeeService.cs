using RefillingStation.Application.DTOs.Employees;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeListItemResponse>> GetAllAsync();
        Task<EmployeeListItemResponse> GetByIdAsync(int id);
        //Task<int> CreateAsync(EmployeeCreateRequest request);
        //Task UpdateAsync(int id, EmployeeCreateRequest request);
        //Task DeleteAsync(int id);
    }
}
