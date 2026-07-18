using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.PayrollPayments;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Exceptions;
using RefillingStation.Tests.Domain.Builders;
using System.Linq.Expressions;

namespace RefillingStation.Tests.Services
{
    public class PayrollPaymentServiceTests
    {
        private readonly Mock<IPayrollPaymentRepository> _payrollPaymentRepository;
        private readonly Mock<IEmployeeRepository> _employeeRepository;
        private readonly Mock<IValidator<PayrollPaymentCreateRequest>> _validator;

        private readonly PayrollPaymentService _payrollPaymentService;

        public PayrollPaymentServiceTests()
        {
            _payrollPaymentRepository = new Mock<IPayrollPaymentRepository>();
            _employeeRepository = new Mock<IEmployeeRepository>();
            _validator = new Mock<IValidator<PayrollPaymentCreateRequest>>();

            _payrollPaymentService = new PayrollPaymentService(
                _payrollPaymentRepository.Object,
                _employeeRepository.Object,
                _validator.Object
            );
        }

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_Should_Return_All_Payments()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(1)
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            var payments = new List<PayrollPayment>
            {
                new PayrollPayment
                {
                    Id = 1,
                    EmployeeId = 1,
                    Employee = employee,
                    PaidDate = new DateOnly(2024, 1, 15),
                    AmountPaid = 1000m,
                    Notes = "Payment 1"
                },
                new PayrollPayment
                {
                    Id = 2,
                    EmployeeId = 1,
                    Employee = employee,
                    PaidDate = new DateOnly(2024, 2, 15),
                    AmountPaid = 1000m,
                    Notes = "Payment 2"
                }
            };

            _payrollPaymentRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(payments);

            // Act
            var result = await _payrollPaymentService.GetAllAsync();

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].AmountPaid.Should().Be(1000m);
            result[0].EmployeeName.Should().Be("John Doe");
            result[1].Id.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Payments_Exist()
        {
            // Arrange
            _payrollPaymentRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<PayrollPayment>());

            // Act
            var result = await _payrollPaymentService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_Should_Map_Employee_Full_Name()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(1)
                .WithFirstName("Jane")
                .WithLastName("Smith")
                .Build();

            var payments = new List<PayrollPayment>
            {
                new PayrollPayment
                {
                    Id = 1,
                    EmployeeId = 1,
                    Employee = employee,
                    PaidDate = new DateOnly(2024, 1, 15),
                    AmountPaid = 2000m,
                    Notes = null
                }
            };

            _payrollPaymentRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(payments);

            // Act
            var result = await _payrollPaymentService.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].EmployeeName.Should().Be("Jane Smith");
        }
        #endregion

        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_Should_Return_Payment_When_Exists()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(1)
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            var payment = new PayrollPayment
            {
                Id = 1,
                EmployeeId = 1,
                Employee = employee,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 1500m,
                Notes = "Monthly payment"
            };

            _payrollPaymentRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(payment);

            // Act
            var result = await _payrollPaymentService.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.AmountPaid.Should().Be(1500m);
            result.EmployeeName.Should().Be("John Doe");
            result.Notes.Should().Be("Monthly payment");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_NotFoundException_When_Payment_Does_Not_Exist()
        {
            // Arrange
            _payrollPaymentRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((PayrollPayment?)null);

            // Act & Assert
            await _payrollPaymentService.Invoking(s => s.GetByIdAsync(999))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Map_All_Properties_Correctly()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(5)
                .WithFirstName("Alice")
                .WithLastName("Johnson")
                .Build();

            var payment = new PayrollPayment
            {
                Id = 10,
                EmployeeId = 5,
                Employee = employee,
                PaidDate = new DateOnly(2024, 3, 20),
                AmountPaid = 3500m,
                Notes = "Quarterly bonus"
            };

            _payrollPaymentRepository
                .Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(payment);

            // Act
            var result = await _payrollPaymentService.GetByIdAsync(10);

            // Assert
            result.Id.Should().Be(10);
            result.EmployeeId.Should().Be(5);
            result.EmployeeName.Should().Be("Alice Johnson");
            result.PaidDate.Should().Be(new DateOnly(2024, 3, 20));
            result.AmountPaid.Should().Be(3500m);
            result.Notes.Should().Be("Quarterly bonus");
        }
        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_Should_Create_Payment_When_Valid_Request_Provided()
        {
            // Arrange
            var request = new PayrollPaymentCreateRequest
            {
                EmployeeId = 1,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 1000m,
                Notes = "Payment"
            };

            var validationResult = new ValidationResult();
            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _payrollPaymentRepository
                .Setup(r => r.AddAsync(It.IsAny<PayrollPayment>()))
                .Callback<PayrollPayment>(p => p.Id = 1)
                .Returns(Task.CompletedTask);

            _payrollPaymentRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _payrollPaymentService.CreateAsync(request);

            // Assert
            result.Should().Be(1);
            _payrollPaymentRepository.Verify(r => r.AddAsync(It.IsAny<PayrollPayment>()), Times.Once);
            _payrollPaymentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_Correct_Properties_On_Created_Payment()
        {
            // Arrange
            var request = new PayrollPaymentCreateRequest
            {
                EmployeeId = 2,
                PaidDate = new DateOnly(2024, 2, 20),
                AmountPaid = 2500m,
                Notes = "Bonus payment"
            };

            var validationResult = new ValidationResult();
            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            PayrollPayment? createdPayment = null;
            _payrollPaymentRepository
                .Setup(r => r.AddAsync(It.IsAny<PayrollPayment>()))
                .Callback<PayrollPayment>(p => createdPayment = p)
                .Returns(Task.CompletedTask);

            _payrollPaymentRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _payrollPaymentService.CreateAsync(request);

            // Assert
            createdPayment.Should().NotBeNull();
            createdPayment!.EmployeeId.Should().Be(2);
            createdPayment.PaidDate.Should().Be(new DateOnly(2024, 2, 20));
            createdPayment.AmountPaid.Should().Be(2500m);
            createdPayment.Notes.Should().Be("Bonus payment");
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_ValidationException_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new PayrollPaymentCreateRequest
            {
                EmployeeId = 0,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 0,
                Notes = null
            };

            var validationFailure = new ValidationFailure("EmployeeId", "Employee ID is required");
            var validationResult = new ValidationResult(new[] { validationFailure });

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act & Assert
            await _payrollPaymentService.Invoking(s => s.CreateAsync(request))
                .Should()
                .ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_NotFoundException_When_Employee_Does_Not_Exist()
        {
            // Arrange
            var request = new PayrollPaymentCreateRequest
            {
                EmployeeId = 999,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 1000m,
                Notes = "Payment"
            };

            var validationResult = new ValidationResult();
            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(false);

            // Act & Assert
            await _payrollPaymentService.Invoking(s => s.CreateAsync(request))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CreateAsync_Should_Return_Created_Payment_Id()
        {
            // Arrange
            var request = new PayrollPaymentCreateRequest
            {
                EmployeeId = 1,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 1000m,
                Notes = "Payment"
            };

            var validationResult = new ValidationResult();
            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _payrollPaymentRepository
                .Setup(r => r.AddAsync(It.IsAny<PayrollPayment>()))
                .Callback<PayrollPayment>(p => p.Id = 42)
                .Returns(Task.CompletedTask);

            _payrollPaymentRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _payrollPaymentService.CreateAsync(request);

            // Assert
            result.Should().Be(42);
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_Should_Update_Payment_When_Valid_Request_Provided()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(1)
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            var existingPayment = new PayrollPayment
            {
                Id = 1,
                EmployeeId = 1,
                Employee = employee,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 1000m,
                Notes = "Old payment"
            };

            var request = new PayrollPaymentCreateRequest
            {
                EmployeeId = 1,
                PaidDate = new DateOnly(2024, 1, 20),
                AmountPaid = 1500m,
                Notes = "Updated payment"
            };

            var validationResult = new ValidationResult();
            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _payrollPaymentRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingPayment);

            _payrollPaymentRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _payrollPaymentService.UpdateAsync(1, request);

            // Assert
            existingPayment.PaidDate.Should().Be(new DateOnly(2024, 1, 20));
            existingPayment.AmountPaid.Should().Be(1500m);
            existingPayment.Notes.Should().Be("Updated payment");
            _payrollPaymentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_NotFoundException_When_Payment_Does_Not_Exist()
        {
            // Arrange
            var request = new PayrollPaymentCreateRequest
            {
                EmployeeId = 1,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 1000m,
                Notes = "Payment"
            };

            var validationResult = new ValidationResult();
            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _payrollPaymentRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((PayrollPayment?)null);

            // Act & Assert
            await _payrollPaymentService.Invoking(s => s.UpdateAsync(999, request))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_ValidationException_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new PayrollPaymentCreateRequest
            {
                EmployeeId = 0,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 0,
                Notes = null
            };

            var validationFailure = new ValidationFailure("AmountPaid", "Amount must be greater than 0");
            var validationResult = new ValidationResult(new[] { validationFailure });

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act & Assert
            await _payrollPaymentService.Invoking(s => s.UpdateAsync(1, request))
                .Should()
                .ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_NotFoundException_When_Employee_Does_Not_Exist()
        {
            // Arrange
            var request = new PayrollPaymentCreateRequest
            {
                EmployeeId = 999,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 1000m,
                Notes = "Payment"
            };

            var validationResult = new ValidationResult();
            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(false);

            // Act & Assert
            await _payrollPaymentService.Invoking(s => s.UpdateAsync(1, request))
                .Should()
                .ThrowAsync<NotFoundException>();
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_Should_Delete_Payment_When_Exists()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(1)
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            var payment = new PayrollPayment
            {
                Id = 1,
                EmployeeId = 1,
                Employee = employee,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 1000m,
                Notes = "Payment"
            };

            _payrollPaymentRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(payment);

            _payrollPaymentRepository
                .Setup(r => r.Remove(It.IsAny<PayrollPayment>()))
                .Verifiable();

            _payrollPaymentRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _payrollPaymentService.DeleteAsync(1);

            // Assert
            _payrollPaymentRepository.Verify(r => r.Remove(payment), Times.Once);
            _payrollPaymentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_NotFoundException_When_Payment_Does_Not_Exist()
        {
            // Arrange
            _payrollPaymentRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((PayrollPayment?)null);

            // Act & Assert
            await _payrollPaymentService.Invoking(s => s.DeleteAsync(999))
                .Should()
                .ThrowAsync<NotFoundException>();
        }
        #endregion

        #region SearchByDateRangeAsync
        [Fact]
        public async Task SearchByDateRangeAsync_Should_Return_Payments_Within_Date_Range()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(1)
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            var request = new DateRangeRequest
            {
                StartDate = new DateOnly(2024, 1, 1),
                EndDate = new DateOnly(2024, 1, 31),
                Page = 1
            };

            var payments = new List<PayrollPayment>
            {
                new PayrollPayment
                {
                    Id = 1,
                    EmployeeId = 1,
                    Employee = employee,
                    PaidDate = new DateOnly(2024, 1, 15),
                    AmountPaid = 1000m,
                    Notes = "Payment 1"
                },
                new PayrollPayment
                {
                    Id = 2,
                    EmployeeId = 1,
                    Employee = employee,
                    PaidDate = new DateOnly(2024, 1, 25),
                    AmountPaid = 1500m,
                    Notes = "Payment 2"
                }
            };

            _payrollPaymentRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync(payments);

            // Act
            var result = await _payrollPaymentService.SearchByDateRangeAsync(request);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result[0].PaidDate.Should().Be(new DateOnly(2024, 1, 15));
            result[1].PaidDate.Should().Be(new DateOnly(2024, 1, 25));
        }

        [Fact]
        public async Task SearchByDateRangeAsync_Should_Return_Empty_List_When_No_Payments_Found()
        {
            // Arrange
            var request = new DateRangeRequest
            {
                StartDate = new DateOnly(2024, 6, 1),
                EndDate = new DateOnly(2024, 6, 30),
                Page = 1
            };

            _payrollPaymentRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync(new List<PayrollPayment>());

            // Act
            var result = await _payrollPaymentService.SearchByDateRangeAsync(request);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchByDateRangeAsync_Should_Pass_Correct_Options_To_Repository()
        {
            // Arrange
            var request = new DateRangeRequest
            {
                StartDate = new DateOnly(2024, 2, 1),
                EndDate = new DateOnly(2024, 2, 29),
                Page = 2
            };

            _payrollPaymentRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync(new List<PayrollPayment>());

            // Act
            await _payrollPaymentService.SearchByDateRangeAsync(request);

            // Assert
            _payrollPaymentRepository.Verify(
                r => r.SearchByDateRangeAsync(It.Is<DateRangeOptions>(o =>
                    o.StartDate == new DateOnly(2024, 2, 1) &&
                    o.EndDate == new DateOnly(2024, 2, 29) &&
                    o.Page == 2
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task SearchByDateRangeAsync_Should_Map_Payments_To_Response()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(2)
                .WithFirstName("Jane")
                .WithLastName("Smith")
                .Build();

            var request = new DateRangeRequest
            {
                StartDate = new DateOnly(2024, 3, 1),
                EndDate = new DateOnly(2024, 3, 31),
                Page = 1
            };

            var payments = new List<PayrollPayment>
            {
                new PayrollPayment
                {
                    Id = 5,
                    EmployeeId = 2,
                    Employee = employee,
                    PaidDate = new DateOnly(2024, 3, 15),
                    AmountPaid = 3000m,
                    Notes = "Monthly salary"
                }
            };

            _payrollPaymentRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync(payments);

            // Act
            var result = await _payrollPaymentService.SearchByDateRangeAsync(request);

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(5);
            result[0].EmployeeId.Should().Be(2);
            result[0].EmployeeName.Should().Be("Jane Smith");
            result[0].AmountPaid.Should().Be(3000m);
        }
        #endregion

        #region Edge Cases
        [Fact]
        public async Task GetAllAsync_Should_Handle_Null_Notes()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(1)
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            var payments = new List<PayrollPayment>
            {
                new PayrollPayment
                {
                    Id = 1,
                    EmployeeId = 1,
                    Employee = employee,
                    PaidDate = new DateOnly(2024, 1, 15),
                    AmountPaid = 1000m,
                    Notes = null
                }
            };

            _payrollPaymentRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(payments);

            // Act
            var result = await _payrollPaymentService.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Notes.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Allow_Null_Notes()
        {
            // Arrange
            var request = new PayrollPaymentCreateRequest
            {
                EmployeeId = 1,
                PaidDate = new DateOnly(2024, 1, 15),
                AmountPaid = 1000m,
                Notes = null
            };

            var validationResult = new ValidationResult();
            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            PayrollPayment? createdPayment = null;
            _payrollPaymentRepository
                .Setup(r => r.AddAsync(It.IsAny<PayrollPayment>()))
                .Callback<PayrollPayment>(p => createdPayment = p)
                .Returns(Task.CompletedTask);

            _payrollPaymentRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _payrollPaymentService.CreateAsync(request);

            // Assert
            createdPayment.Should().NotBeNull();
            createdPayment!.Notes.Should().BeNull();
        }

        [Fact]
        public async Task SearchByDateRangeAsync_Should_Handle_Same_Start_And_End_Date()
        {
            // Arrange
            var employee = new EmployeeBuilder()
                .WithId(1)
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            var request = new DateRangeRequest
            {
                StartDate = new DateOnly(2024, 1, 15),
                EndDate = new DateOnly(2024, 1, 15),
                Page = 1
            };

            var payments = new List<PayrollPayment>
            {
                new PayrollPayment
                {
                    Id = 1,
                    EmployeeId = 1,
                    Employee = employee,
                    PaidDate = new DateOnly(2024, 1, 15),
                    AmountPaid = 500m,
                    Notes = "Partial payment"
                }
            };

            _payrollPaymentRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync(payments);

            // Act
            var result = await _payrollPaymentService.SearchByDateRangeAsync(request);

            // Assert
            result.Should().HaveCount(1);
            result[0].PaidDate.Should().Be(new DateOnly(2024, 1, 15));
        }
        #endregion
    }
}
