using RefillingStation.Domain.Enitities;
using RefillingStation.Domain.Enums;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public EmployeeRole Role { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public EmploymentType EmploymentType { get; set; }
        public bool IsActive { get; private set; } = true;
        public DateTime? DeactivatedAt { get; set; }

        // Relationships
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<PayrollEntry> PayrollEntries { get; set; } = new List<PayrollEntry>();
        public ICollection<PayrollPayment> PayrollPayments { get; set; } = new List<PayrollPayment>();

        public void Activate()
        {
            if (IsActive)
                throw new DomainException(DomainErrorCodes.EmployeeCode.AlreadyActive, "Employee is already active.");

            IsActive = true;
            DeactivatedAt = null;
        }
        public void Deactivate()
        {
            if (!IsActive)
                throw new DomainException(DomainErrorCodes.EmployeeCode.AlreadyInactive, "Employee is already inactive.");

            IsActive = false;
            DeactivatedAt = DateTime.UtcNow;
        }

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
    }
}
