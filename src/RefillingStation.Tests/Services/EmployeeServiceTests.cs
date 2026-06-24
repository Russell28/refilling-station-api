using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RefillingStation.Application.DTOs.Employees;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Enums;
using RefillingStation.Domain.Exceptions;
using RefillingStation.Tests.Domain.Builders;
using RefillingStation.Tests.Services.Builders;

namespace RefillingStation.Tests.Services
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository> _employeeRepository;
        private readonly Mock<IValidator<EmployeeCreateRequest>> _validator;
        private readonly Mock<IMemoryCache> _mockMemoryCache;

        private readonly EmployeeService _employeeService;

        public EmployeeServiceTests()
        {
            _employeeRepository = new Mock<IEmployeeRepository>();
            _validator = new Mock<IValidator<EmployeeCreateRequest>>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            _employeeService = new EmployeeService(
                _employeeRepository.Object,
                _mockMemoryCache.Object,
                _validator.Object
            );
        }

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_Should_Return_Cached_Results_When_Cache_Hit()
        {
            // Arrange
            var cachedResult = new List<EmployeeDetailResponse>
            {
                new EmployeeDetailResponse(1, "John", "Doe", "1234567890", "DeliveryRider", "Permanent", true, null)
            };

            object cachedValue = cachedResult;
            _mockMemoryCache
                .Setup(c => c.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(true);

            // Act
            var result = await _employeeService.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(cachedResult);
            _employeeRepository.Verify(r => r.GetAllAsync(), Times.Never);
        }
        #endregion

        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_Should_Return_Employee_When_Exists()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(5)
                .WithFirstName("Mike")
                .WithLastName("Johnson")
                .WithPhoneNumber("9876543210")
                .Build();

            _employeeRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(employee);

            // Act
            var result = await _employeeService.GetByIdAsync(5);

            // Assert
            result.Id.Should().Be(5);
            result.FirstName.Should().Be("Mike");
            result.LastName.Should().Be("Johnson");
            result.PhoneNumber.Should().Be("9876543210");
            result.Role.Should().Be(employee.Role.ToString());
            result.EmploymentType.Should().Be(employee.EmploymentType.ToString());
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_NotFoundException_When_Employee_Does_Not_Exist()
        {
            // Arrange
            _employeeRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Employee?)null);

            // Act
            Func<Task> action = async () => await _employeeService.GetByIdAsync(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Map_IsActive_And_DeactivatedAt_Properties()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(10)
                .WithFirstName("Sarah")
                .WithLastName("Williams")
                .Build();

            employee.Deactivate();

            _employeeRepository
                .Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(employee);

            // Act
            var result = await _employeeService.GetByIdAsync(10);

            // Assert
            result.IsActive.Should().BeFalse();
            result.DeactivatedAt.Should().NotBeNull();
        }
        #endregion

        #region GetActiveEmployeesAsync
        [Fact]
        public async Task GetActiveEmployeesAsync_Should_Return_Cached_Results_When_Cache_Hit()
        {
            // Arrange
            var cachedResult = new List<EmployeeListItemResponse>
            {
                new EmployeeListItemResponse(1, "Charlie Davis")
            };

            object cachedValue = cachedResult;
            _mockMemoryCache
                .Setup(c => c.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(true);

            // Act
            var result = await _employeeService.GetActiveEmployeesAsync();

            // Assert
            result.Should().BeEquivalentTo(cachedResult);
            _employeeRepository.Verify(r => r.GetActiveAsync(), Times.Never);
        }
        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_Should_Create_Employee_When_Valid_Request_Provided()
        {
            // Arrange
            var request = new EmployeeCreateRequestBuilder()
                .WithFirstName("NewEmployee")
                .WithLastName("Test")
                .WithPhoneNumber("5555555555")
                .WithRole(EmployeeRole.Refiller)
                .WithEmploymentType(EmploymentType.OnCall)
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.AddAsync(It.IsAny<Employee>()))
                .Callback<Employee>(e => e.Id = 1)
                .Returns(Task.CompletedTask);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _employeeService.CreateAsync(request);

            // Assert
            result.Should().BeGreaterThan(0);
            result.Should().Be(1);
            _employeeRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
            _employeeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_Correct_Properties_On_Created_Employee()
        {
            // Arrange
            var request = new EmployeeCreateRequestBuilder()
                .WithFirstName("Emma")
                .WithLastName("Wilson")
                .WithPhoneNumber("1111111111")
                .WithRole(EmployeeRole.Manager)
                .WithEmploymentType(EmploymentType.Permanent)
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            Employee? capturedEmployee = null;
            _employeeRepository
                .Setup(r => r.AddAsync(It.IsAny<Employee>()))
                .Callback<Employee>(e => capturedEmployee = e)
                .Returns(Task.CompletedTask);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _employeeService.CreateAsync(request);

            // Assert
            capturedEmployee.Should().NotBeNull();
            capturedEmployee!.FirstName.Should().Be("Emma");
            capturedEmployee.LastName.Should().Be("Wilson");
            capturedEmployee.PhoneNumber.Should().Be("1111111111");
            capturedEmployee.Role.Should().Be(EmployeeRole.Manager);
            capturedEmployee.EmploymentType.Should().Be(EmploymentType.Permanent);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_ValidationException_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new EmployeeCreateRequestBuilder()
                .WithFirstName("")
                .Build();

            var validationFailure = new List<ValidationFailure>
            {
                new ValidationFailure("FirstName", "First name is required")
            };

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailure));

            // Act
            Func<Task> action = async () => await _employeeService.CreateAsync(request);

            // Assert
            await action.Should().ThrowAsync<ValidationException>();
            _employeeRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_Should_Clear_Cache_After_Creating_Employee()
        {
            // Arrange
            var request = new EmployeeCreateRequestBuilder().Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.AddAsync(It.IsAny<Employee>()))
                .Callback<Employee>(e => e.Id = 5)
                .Returns(Task.CompletedTask);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _employeeService.CreateAsync(request);

            // Assert
            _mockMemoryCache.Verify(c => c.Remove("EmployeeList"), Times.Once);
            _mockMemoryCache.Verify(c => c.Remove("ActiveEmployeeList"), Times.Once);
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_Should_Update_Employee_When_Valid_Request_Provided()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(5)
                .WithFirstName("Old")
                .WithLastName("Name")
                .WithRole(EmployeeRole.DeliveryRider)
                .Build();

            var request = new EmployeeCreateRequestBuilder()
                .WithFirstName("Updated")
                .WithLastName("Name")
                .WithRole(EmployeeRole.Supervisor)
                .WithPhoneNumber("9999999999")
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(employee);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _employeeService.UpdateAsync(5, request);

            // Assert
            employee.FirstName.Should().Be("Updated");
            employee.LastName.Should().Be("Name");
            employee.Role.Should().Be(EmployeeRole.Supervisor);
            employee.PhoneNumber.Should().Be("9999999999");
            _employeeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_ValidationException_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new EmployeeCreateRequestBuilder()
                .WithFirstName("")
                .Build();

            var validationFailure = new List<ValidationFailure>
            {
                new ValidationFailure("FirstName", "First name is required")
            };

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailure));

            // Act
            Func<Task> action = async () => await _employeeService.UpdateAsync(5, request);

            // Assert
            await action.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_NotFoundException_When_Employee_Does_Not_Exist()
        {
            // Arrange
            var request = new EmployeeCreateRequestBuilder().Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Employee?)null);

            // Act
            Func<Task> action = async () => await _employeeService.UpdateAsync(999, request);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Clear_Cache_After_Updating_Employee()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(5)
                .Build();

            var request = new EmployeeCreateRequestBuilder().Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(employee);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _employeeService.UpdateAsync(5, request);

            // Assert
            _mockMemoryCache.Verify(c => c.Remove("EmployeeList"), Times.Once);
            _mockMemoryCache.Verify(c => c.Remove("ActiveEmployeeList"), Times.Once);
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_Should_Delete_Employee_When_Exists()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(5)
                .Build();

            _employeeRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(employee);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _employeeService.DeleteAsync(5);

            // Assert
            _employeeRepository.Verify(r => r.Remove(employee), Times.Once);
            _employeeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_NotFoundException_When_Employee_Does_Not_Exist()
        {
            // Arrange
            _employeeRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Employee?)null);

            // Act
            Func<Task> action = async () => await _employeeService.DeleteAsync(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
            _employeeRepository.Verify(r => r.Remove(It.IsAny<Employee>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_Should_Clear_Cache_After_Deleting_Employee()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(5)
                .Build();

            _employeeRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(employee);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _employeeService.DeleteAsync(5);

            // Assert
            _mockMemoryCache.Verify(c => c.Remove("EmployeeList"), Times.Once);
            _mockMemoryCache.Verify(c => c.Remove("ActiveEmployeeList"), Times.Once);
        }
        #endregion

        #region ActivateAsync
        [Fact]
        public async Task ActivateAsync_Should_Activate_Employee_When_Exists()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(5)
                .Build();

            employee.Deactivate();

            _employeeRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(employee);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _employeeService.ActivateAsync(5);

            // Assert
            employee.IsActive.Should().BeTrue();
            _employeeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ActivateAsync_Should_Throw_NotFoundException_When_Employee_Does_Not_Exist()
        {
            // Arrange
            _employeeRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Employee?)null);

            // Act
            Func<Task> action = async () => await _employeeService.ActivateAsync(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ActivateAsync_Should_Clear_Cache_After_Activating_Employee()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(5)
                .Build();

            employee.Deactivate();

            _employeeRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(employee);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _employeeService.ActivateAsync(5);

            // Assert
            _mockMemoryCache.Verify(c => c.Remove("EmployeeList"), Times.Once);
            _mockMemoryCache.Verify(c => c.Remove("ActiveEmployeeList"), Times.Once);
        }
        #endregion

        #region DeactivateAsync
        [Fact]
        public async Task DeactivateAsync_Should_Deactivate_Employee_When_Exists()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(5)
                .Build();

            _employeeRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(employee);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _employeeService.DeactivateAsync(5);

            // Assert
            employee.IsActive.Should().BeFalse();
            employee.DeactivatedAt.Should().NotBeNull();
            _employeeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactivateAsync_Should_Throw_NotFoundException_When_Employee_Does_Not_Exist()
        {
            // Arrange
            _employeeRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Employee?)null);

            // Act
            Func<Task> action = async () => await _employeeService.DeactivateAsync(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DeactivateAsync_Should_Clear_Cache_After_Deactivating_Employee()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(5)
                .Build();

            _employeeRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(employee);

            _employeeRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _employeeService.DeactivateAsync(5);

            // Assert
            _mockMemoryCache.Verify(c => c.Remove("EmployeeList"), Times.Once);
            _mockMemoryCache.Verify(c => c.Remove("ActiveEmployeeList"), Times.Once);
        }
        #endregion
    }
}
