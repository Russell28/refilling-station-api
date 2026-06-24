using RefillingStation.Application.DTOs.Employees;
using RefillingStation.Domain.Enums;

namespace RefillingStation.Tests.Services.Builders
{
    public class EmployeeCreateRequestBuilder
    {
        private string _firstName = "John";
        private string _lastName = "Doe";
        private string _phoneNumber = "1234567890";
        private EmployeeRole _role = EmployeeRole.DeliveryRider;
        private EmploymentType _employmentType = EmploymentType.Permanent;

        public EmployeeCreateRequestBuilder WithFirstName(string firstName)
        {
            _firstName = firstName;
            return this;
        }

        public EmployeeCreateRequestBuilder WithLastName(string lastName)
        {
            _lastName = lastName;
            return this;
        }

        public EmployeeCreateRequestBuilder WithPhoneNumber(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public EmployeeCreateRequestBuilder WithRole(EmployeeRole role)
        {
            _role = role;
            return this;
        }

        public EmployeeCreateRequestBuilder WithEmploymentType(EmploymentType employmentType)
        {
            _employmentType = employmentType;
            return this;
        }

        public EmployeeCreateRequest Build()
        {
            return new EmployeeCreateRequest
            {
                FirstName = _firstName,
                LastName = _lastName,
                PhoneNumber = _phoneNumber,
                Role = _role,
                EmploymentType = _employmentType
            };
        }
    }
}
