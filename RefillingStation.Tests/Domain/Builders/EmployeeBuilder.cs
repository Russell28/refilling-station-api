using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Enums;

namespace RefillingStation.Tests.Domain.Builders
{
    public class EmployeeBuilder
    {
        private Employee _employee = new Employee
        {
            FirstName = "First",
            LastName = "Last",
            Role = Role.DeliveryRider
        };

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

        public EmployeeBuilder WithRole(Role role)
        {
            _employee.Role = role;
            return this;
        }

        public Employee Build()
        {
            return _employee;
        }
    }
}
