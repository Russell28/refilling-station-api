using RefillingStation.Domain.Enums;

namespace RefillingStation.Application.DTOs.Employees
{
    public record EmployeeListItemResponse(
        int Id,
        string Name
    );
    
}
