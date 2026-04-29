using RefillingStation.Api.Features.Employees.enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace RefillingStation.Api.Entities
{
    public class Employee : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Role Role { get; set; }
        public string? PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Relationships
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
