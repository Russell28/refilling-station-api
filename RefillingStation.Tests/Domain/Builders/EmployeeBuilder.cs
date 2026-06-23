using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Enums;

namespace RefillingStation.Tests.Domain.Builders
{
    public class EmployeeBuilder
    {
        private Employee _employee = new Employee
        {
            Id = 1,
            FirstName = "First",
            LastName = "Last",
            PhoneNumber = "1234567890",
            Role = EmployeeRole.DeliveryRider,
            EmploymentType = EmploymentType.Permanent
        };

        public EmployeeBuilder WithId(int id)
        {
            _employee.Id = id;
            return this;
        }

        public EmployeeBuilder WithFirstName(string firstName)
        {
            _employee.FirstName = firstName;
            return this;
        }

        public EmployeeBuilder WithLastName(string lastName)
        {
            _employee.LastName = lastName;
            return this;
        }

        public EmployeeBuilder WithPhoneNumber(string phoneNumber)
        {
            _employee.PhoneNumber = phoneNumber;
            return this;
        }

        public EmployeeBuilder WithRole(EmployeeRole role)
        {
            _employee.Role = role;
            return this;
        }

        public EmployeeBuilder WithEmploymentType(EmploymentType employmentType)
        {
            _employee.EmploymentType = employmentType;
            return this;
        }

        public Employee Build()
        {
            return _employee;
        }
    }
}
