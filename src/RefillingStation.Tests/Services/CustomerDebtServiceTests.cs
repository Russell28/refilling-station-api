using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.CustomerDebts;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Exceptions;
using RefillingStation.Tests.Domain.Builders;
using RefillingStation.Tests.Services.Builders;
using System.Linq.Expressions;

namespace RefillingStation.Tests.Services
{
    public class CustomerDebtServiceTests
    {
        private readonly Mock<ICustomerDebtRepository> _customerDebtRepository;
        private readonly Mock<ICustomerRepository> _customerRepository;
        private readonly Mock<IValidator<CustomerDebtCreateRequest>> _validator;

        private readonly CustomerDebtService _debtService;

        public CustomerDebtServiceTests()
        {
            _customerDebtRepository = new Mock<ICustomerDebtRepository>();
            _customerRepository = new Mock<ICustomerRepository>();
            _validator = new Mock<IValidator<CustomerDebtCreateRequest>>();

            _debtService = new CustomerDebtService(
                    _customerDebtRepository.Object,
                    _customerRepository.Object,
                    _validator.Object
                );
        }

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_Should_Map_All_Records()
        {
            var debts = new List<CustomerDebtEntry>
            {
                new CustomerDebtBuilder()
                    .WithCustomer(new CustomerBuilder().WithName("First").Build())
                    .Build(),

                new CustomerDebtBuilder()
                    .WithCustomer(new CustomerBuilder().WithName("Second").Build())
                    .Build()
            };

            _customerDebtRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(debts);

            // Act
            var result = await _debtService.GetAllAsync();

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Records_Exist()
        {
            _customerDebtRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync([]);

            // Act
            var result = await _debtService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }
        #endregion

        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_Should_ReturnMappedResponse_WhenDebtExists()
        {
            // Arrange
            var debt = new CustomerDebtBuilder()
                .WithCustomer(new CustomerBuilder().Build())
                .Build();

            _customerDebtRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(debt);

            var expected = new CustomerDebtDetailResponse(
                    debt.Id,
                    debt.Date,
                    debt.CustomerId,
                    debt.Customer.Name,
                    debt.Amount,
                    debt.Notes
                );

            // Act
            var result = await _debtService.GetByIdAsync(1);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_DebtNotFound()
        {
            // Arrange
            _customerDebtRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((CustomerDebtEntry?) null);

            // Act
            Func<Task> act = () => _debtService.GetByIdAsync(1);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_Should_ReturnId_When_ValidRequest()
        {
            var request = new CustomerDebtCreateRequestBuilder()
                .WithAmount(100)
                .Build();

            _validator
                .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _customerRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(true);

            CustomerDebtEntry captured = null;

            _customerDebtRepository
                .Setup(r => r.AddAsync(It.IsAny<CustomerDebtEntry>()))
                .Callback<CustomerDebtEntry>(debt => captured = debt)
                .Returns(Task.CompletedTask);

            _customerDebtRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            var expected = new CustomerDebtEntry
            {
                Date = request.Date,
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Notes = request.Notes,
            };


            // Act
            var result = await _debtService.CreateAsync(request);

            // Assert
            result.Should().Be(captured.Id);
            captured.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_InvalidRequest()
        {
            var request = new CustomerDebtCreateRequestBuilder()
                .WithAmount(0)
                .Build();

            var validationResult = new ValidationResult(
                new[]
                {
                    new ValidationFailure("Amount", "Cannot be zero")
                });

            _validator
                .Setup(r => r.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            Func<Task> act = () => _debtService.CreateAsync(request);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_CustomerNotFound()
        {
            var request = new CustomerDebtCreateRequestBuilder()
                .WithAmount(100)
                .Build();

            _validator
                .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _customerRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = () => _debtService.CreateAsync(request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_Should_UpdateDebt_When_RequestIsValid()
        {
            var request = new CustomerDebtCreateRequestBuilder()
                .WithAmount(100)
                .Build();

            var debt = new CustomerDebtBuilder()
                .Build();

            _validator
                .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _customerRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(true);

            _customerDebtRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(debt);

            var expected = new CustomerDebtEntry
            {
                Id = 1,
                Date = request.Date,
                Amount = request.Amount,
                CustomerId = request.CustomerId,
                Notes = request.Notes,
            };

            // Act
            await _debtService.UpdateAsync(1, request);

            // Assert
            debt.Should().BeEquivalentTo(expected);

            _customerDebtRepository
                .Verify(r => r.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_RequestIsInValid()
        {
            var request = new CustomerDebtCreateRequestBuilder()
                .WithAmount(0)
                .Build();

            var validationResult = new ValidationResult(
                new[]
                {
                    new ValidationFailure("Amount", "Cannot be zero")
                });

            _validator
                .Setup(r => r.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            Func<Task> act = () => _debtService.UpdateAsync(1, request);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_CustomerNotFound()
        {
            var request = new CustomerDebtCreateRequestBuilder()
                .WithAmount(100)
                .Build();

            _validator
                .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _customerRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = () => _debtService.UpdateAsync(1, request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_DebtNotFound()
        {
            var request = new CustomerDebtCreateRequestBuilder()
                .WithAmount(100)
                .Build();

            var debt = new CustomerDebtBuilder()
                .Build();

            _validator
                .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _customerRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(true);

            _customerDebtRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((CustomerDebtEntry?) null);

            // Act
            Func<Task> act = () => _debtService.UpdateAsync(1, request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_Should_Remove_And_Save()
        {
            var debt = new CustomerDebtBuilder().Build();

            _customerDebtRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(debt);

            // Act
            await _debtService.DeleteAsync(1);

            // Assert
            _customerDebtRepository
                .Verify(r => r.Remove(debt), Times.Once);

            _customerDebtRepository
                .Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_DebtNotFound()
        {
            _customerDebtRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((CustomerDebtEntry?) null);

            // Act
            Func<Task> act = () => _debtService.DeleteAsync(1);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region SearchByDateRangeAsync
        [Fact]
        public async Task SearchByDateRangeAsync_Should_Map_Trips_To_Response()
        {
            var debt = new CustomerDebtBuilder()
                .WithAmount(100)
                .WithCustomer(new CustomerBuilder().WithName("test").Build())
                .Build();

            _customerDebtRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync([debt]);

            // Act
            var result = await _debtService.SearchByDateRangeAsync(new DateRangeRequest());

            // Assert
            result.Should().HaveCount(1);
            result[0].Amount.Should().Be(100);
            result[0].CustomerName.Should().Be(debt.Customer.Name);
        }

        [Fact]
        public async Task SearchByDateRangeAsync_Should_Return_Empty_List_When_No_Trips_Found()
        {
            _customerDebtRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync([]);

            // Act
            var result = await _debtService.SearchByDateRangeAsync(new DateRangeRequest());

            // Assert
            result.Should().BeEmpty();
        }
#endregion




    }
}
