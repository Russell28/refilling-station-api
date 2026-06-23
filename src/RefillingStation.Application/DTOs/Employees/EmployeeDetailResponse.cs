namespace RefillingStation.Application.DTOs.Employees
{
    public sealed record EmployeeDetailResponse(
        int Id,
        string FirstName,
        string LastName,
        string PhoneNumber,
        string Role,
        string EmploymentType,
        bool IsActive,
        DateTime? DeactivatedAt
    );
}
