using RefillingStation.Domain.Enums;

namespace RefillingStation.Domain.Entities
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

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
    }
}
