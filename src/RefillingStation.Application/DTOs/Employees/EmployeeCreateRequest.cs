using RefillingStation.Domain.Enums;

namespace RefillingStation.Application.DTOs.Employees
{
    public class EmployeeCreateRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public EmployeeRole Role { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
