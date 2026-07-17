using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.PayrollEntries;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Exceptions;
using RefillingStation.Tests.Domain.Builders;
using RefillingStation.Tests.Services.Builders;
using System.Linq.Expressions;

namespace RefillingStation.Tests.Services
{
    public class PayrollServiceTests
    {
        private readonly Mock<IPayrollEntryRepository> _payrollRepository;
        private readonly Mock<IEmployeeRepository> _employeeRepository;
        private readonly Mock<IValidator<PayrollEntryCreateRequest>> _validator;

        private readonly PayrollEntryService _payrollService;

        public PayrollServiceTests()
        {
            _payrollRepository = new Mock<IPayrollEntryRepository>();
            _employeeRepository = new Mock<IEmployeeRepository>();
            _validator = new Mock<IValidator<PayrollEntryCreateRequest>>();

            _payrollService = new PayrollEntryService(
                _payrollRepository.Object,
                _employeeRepository.Object,
                _validator.Object
            );
        }

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_Should_Map_All_Records()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            var payrolls = new List<PayrollEntry>
            {
                new PayrollBuilder()
                    .WithId(1)
                    .WithEmployee(employee)
                    .WithSalaryAmount(5000m)
                    .WithCashPaid(5000m)
                    .Build(),
                new PayrollBuilder()
                    .WithId(2)
                    .WithEmployee(employee)
                    .WithSalaryAmount(5000m)
                    .WithCashPaid(4500m)
                    .Build()
            };

            _payrollRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(payrolls);

            // Act
            var result = await _payrollService.GetAllAsync();

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].SalaryAmount.Should().Be(5000m);
            result[0].EmployeeName.Should().Be("John Doe");
            result[1].Id.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Records_Exist()
        {
            // Arrange
            _payrollRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync([]);

            // Act
            var result = await _payrollService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }
        #endregion

        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_Should_Return_MappedResponse_When_Payroll_Exists()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithFirstName("Jane")
                .WithLastName("Smith")
                .Build();

            var payroll = new PayrollBuilder()
                .WithId(5)
                .WithEmployee(employee)
                .WithSalaryAmount(6000m)
                .WithCashPaid(6000m)
                .WithNotes("Monthly salary")
                .Build();

            _payrollRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(payroll);

            var expected = new PayrollEntryDetailResponse(
                payroll.Id,
                payroll.EarnedDate,
                payroll.EmployeeId,
                payroll.Employee.FullName,
                payroll.SalaryAmount,
                payroll.Notes
            );

            // Act
            var result = await _payrollService.GetByIdAsync(5);

            // Assert
            result.Should().BeEquivalentTo(expected);
            result.Id.Should().Be(5);
            result.SalaryAmount.Should().Be(6000m);
            result.EmployeeName.Should().Be("Jane Smith");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_NotFoundException_When_Payroll_Does_Not_Exist()
        {
            // Arrange
            _payrollRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((PayrollEntry?)null);

            // Act
            Func<Task> action = async () => await _payrollService.GetByIdAsync(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_Should_Create_Payroll_When_Valid_Request_Provided()
        {
            // Arrange
            var request = new PayrollCreateRequestBuilder()
                .WithSalaryAmount(5500m)
                .WithCashPaid(5500m)
                .WithEmployeeId(1)
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _payrollRepository
                .Setup(r => r.AddAsync(It.IsAny<PayrollEntry>()))
                .Callback<PayrollEntry>(p => p.Id = 1)
                .Returns(Task.CompletedTask);

            _payrollRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _payrollService.CreateAsync(request);

            // Assert
            result.Should().BeGreaterThan(0);
            result.Should().Be(1);
            _payrollRepository.Verify(r => r.AddAsync(It.IsAny<PayrollEntry>()), Times.Once);
            _payrollRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_ValidationException_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new PayrollCreateRequestBuilder()
                .WithSalaryAmount(-5000m)
                .Build();

            var validationFailure = new List<ValidationFailure>
            {
                new ValidationFailure("SalaryAmount", "Salary amount must be greater than zero")
            };

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailure));

            // Act
            Func<Task> action = async () => await _payrollService.CreateAsync(request);

            // Assert
            await action.Should().ThrowAsync<ValidationException>();
            _payrollRepository.Verify(r => r.AddAsync(It.IsAny<PayrollEntry>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_NotFoundException_When_Employee_Does_Not_Exist()
        {
            // Arrange
            var request = new PayrollCreateRequestBuilder()
                .WithEmployeeId(999)
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> action = async () => await _payrollService.CreateAsync(request);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
            _payrollRepository.Verify(r => r.AddAsync(It.IsAny<PayrollEntry>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_Correct_Properties_On_Created_Payroll()
        {
            // Arrange
            var earnedDate = new DateOnly(2024, 1, 1);
            var paidDate = new DateOnly(2024, 1, 5);
            var request = new PayrollCreateRequestBuilder()
                .WithEarnedDate(earnedDate)
                .WithPaidDate(paidDate)
                .WithEmployeeId(2)
                .WithSalaryAmount(7500m)
                .WithCashPaid(7500m)
                .WithNotes("January salary")
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            PayrollEntry? capturedPayroll = null;
            _payrollRepository
                .Setup(r => r.AddAsync(It.IsAny<PayrollEntry>()))
                .Callback<PayrollEntry>(p => capturedPayroll = p)
                .Returns(Task.CompletedTask);

            _payrollRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _payrollService.CreateAsync(request);

            // Assert
            capturedPayroll.Should().NotBeNull();
            capturedPayroll!.EarnedDate.Should().Be(earnedDate);
            capturedPayroll.EmployeeId.Should().Be(2);
            capturedPayroll.SalaryAmount.Should().Be(7500m);
            capturedPayroll.Notes.Should().Be("January salary");
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_Should_Update_Payroll_When_Valid_Request_Provided()
        {
            // Arrange
            var employee = new EmployeeBuilder().Build();

            var existingPayroll = new PayrollBuilder()
                .WithId(5)
                .WithEmployee(employee)
                .WithSalaryAmount(5000m)
                .WithCashPaid(5000m)
                .Build();

            var request = new PayrollCreateRequestBuilder()
                .WithSalaryAmount(5500m)
                .WithCashPaid(5250m)
                .WithNotes("Updated payroll")
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _payrollRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(existingPayroll);

            _payrollRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _payrollService.UpdateAsync(5, request);

            // Assert
            existingPayroll.SalaryAmount.Should().Be(5500m);
            existingPayroll.Notes.Should().Be("Updated payroll");
            _payrollRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_NotFoundException_When_Payroll_Does_Not_Exist()
        {
            // Arrange
            var request = new PayrollCreateRequestBuilder().Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _payrollRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((PayrollEntry?)null);

            // Act
            Func<Task> action = async () => await _payrollService.UpdateAsync(999, request);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_ValidationException_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new PayrollCreateRequestBuilder()
                .WithSalaryAmount(-5000m)
                .Build();

            var validationFailure = new List<ValidationFailure>
            {
                new ValidationFailure("SalaryAmount", "Salary amount must be greater than zero")
            };

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailure));

            // Act
            Func<Task> action = async () => await _payrollService.UpdateAsync(5, request);

            // Assert
            await action.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_NotFoundException_When_Employee_Does_Not_Exist()
        {
            // Arrange
            var employee = new EmployeeBuilder().Build();
            var existingPayroll = new PayrollBuilder()
                .WithId(5)
                .WithEmployee(employee)
                .Build();

            var request = new PayrollCreateRequestBuilder()
                .WithEmployeeId(999)
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> action = async () => await _payrollService.UpdateAsync(5, request);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_Should_Delete_Payroll_When_Exists()
        {
            // Arrange
            var payroll = new PayrollBuilder()
                .WithId(5)
                .Build();

            _payrollRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(payroll);

            _payrollRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _payrollService.DeleteAsync(5);

            // Assert
            _payrollRepository.Verify(r => r.Remove(payroll), Times.Once);
            _payrollRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_NotFoundException_When_Payroll_Does_Not_Exist()
        {
            // Arrange
            _payrollRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((PayrollEntry?)null);

            // Act
            Func<Task> action = async () => await _payrollService.DeleteAsync(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
            _payrollRepository.Verify(r => r.Remove(It.IsAny<PayrollEntry>()), Times.Never);
        }
        #endregion

        #region SearchByDateRangeAsync
        [Fact]
        public async Task SearchByDateRangeAsync_Should_Return_Payrolls_Within_Date_Range()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithFirstName("Mike")
                .WithLastName("Johnson")
                .Build();

            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            var payrolls = new List<PayrollEntry>
            {
                new PayrollBuilder()
                    .WithId(1)
                    .WithEarnedDate(new DateOnly(2024, 1, 15))
                    .WithEmployee(employee)
                    .Build(),
                new PayrollBuilder()
                    .WithId(2)
                    .WithEarnedDate(new DateOnly(2024, 1, 20))
                    .WithEmployee(employee)
                    .Build()
            };

            var request = new DateRangeRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                Page = 1
            };

            _payrollRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync(payrolls);

            // Act
            var result = await _payrollService.SearchByDateRangeAsync(request);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].EmployeeName.Should().Be("Mike Johnson");
            result[1].Id.Should().Be(2);
        }

        [Fact]
        public async Task SearchByDateRangeAsync_Should_Return_Empty_List_When_No_Payrolls_In_Range()
        {
            // Arrange
            var request = new DateRangeRequest
            {
                StartDate = new DateOnly(2024, 12, 1),
                EndDate = new DateOnly(2024, 12, 31),
                Page = 1
            };

            _payrollRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync([]);

            // Act
            var result = await _payrollService.SearchByDateRangeAsync(request);

            // Assert
            result.Should().BeEmpty();
        }
        #endregion
    }
}
