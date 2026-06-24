using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.Trips;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Enums;
using RefillingStation.Domain.Exceptions;
using RefillingStation.Tests.Domain.Builders;
using RefillingStation.Tests.Services.Builders;
using System.Linq.Expressions;

namespace RefillingStation.Tests.Services
{
    public class TripServiceTests
    {
        private readonly Mock<ITripRepository> _tripRepository;
        private readonly Mock<IEmployeeRepository> _employeeRepository;
        private readonly Mock<IValidator<TripCreateRequest>> _createValidator;
        private readonly Mock<IValidator<TripUpdateRequest>> _updateValidator;

        private readonly TripService _tripService;

        public TripServiceTests()
        {
            _tripRepository = new Mock<ITripRepository>();
            _employeeRepository = new Mock<IEmployeeRepository>();
            _createValidator = new Mock<IValidator<TripCreateRequest>>();
            _updateValidator = new Mock<IValidator<TripUpdateRequest>>();

            _tripService = new TripService(
                    _tripRepository.Object,
                    _employeeRepository.Object,
                    _createValidator.Object,
                    _updateValidator.Object
                );
        }

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_Should_Return_All_Trips()
        {
            // Arrange
            var user1 = new TripBuilder()
                    .WithTripNumber(1)
                    .WithEmployee(new EmployeeBuilder().Build())
                    .Build();

            var user2 = new TripBuilder()
                    .WithTripNumber(2)
                    .WithEmployee(new EmployeeBuilder().Build())
                    .Build();

            var trips = new List<Trip>
            {
                user1, user2
            };

            _tripRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(trips);

            // Act
            var result = await _tripService.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            _tripRepository
                .Verify(r => r.GetAllAsync(), Times.Once());
        }
        #endregion

        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_Should_Return_Trip()
        {
            var employee = new EmployeeBuilder().Build();

            var trip = new TripBuilder()
                .WithEmployee(employee)
                .Build();

            _tripRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(trip);

            var result = await _tripService.GetByIdAsync(1);

            result.Date.Should().Be(trip.Date);
            result.TripNumber.Should().Be(trip.TripNumber);
            result.EmployeeName.Should().Be(employee.FullName);

            _tripRepository
                .Verify(r => r.GetByIdAsync(1), Times.Once());
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_NotFound()
        {
            _tripRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Trip?) null);

            Func<Task> act = () => _tripService.GetByIdAsync(1);
            
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_Should_Add_And_Save()
        {
            var request = new TripCreateRequestBuilder().Build();

            _createValidator
                .Setup(x => x.ValidateAsync(
                    request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _tripRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Trip, bool>>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _tripService.CreateAsync(request);

            // Assert
            _tripRepository
                .Verify(r => r.AddAsync(It.IsAny<Trip>()), Times.Once);
            _tripRepository
                .Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Request_Is_invalid()
        {
            var request = new TripCreateRequestBuilder()
                .WithTripNumber(0)
                .Build();

            var validationResult = new ValidationResult(
                new[]
                {
                    new ValidationFailure("TripNumber", "Should be greater than 0")
                });

            _createValidator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            Func<Task> act = () => _tripService.CreateAsync(request);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Employee_NotFound()
        {
            var request = new TripCreateRequestBuilder()
                .Build();

            _createValidator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(false);

            Func<Task> act = () => _tripService.CreateAsync(request);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_TripNumber_Already_Exists_In_The_Same_Date()
        {
            var request = new TripCreateRequestBuilder()
                .WithTripNumber(1)
                .Build();

            _createValidator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _tripRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Trip, bool>>>()))
                .ReturnsAsync(true);

            Func<Task> act = () => _tripService.CreateAsync(request);

            await act.Should().ThrowAsync<ConflictException>();
        }
        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_Should_Update_And_Save()
        {
            var request = new TripUpdateRequestBuilder()
                .WithActualCashCollected(500)
                .Build();

            var trip = new TripBuilder()
                .WithActualCashCollected(100)
                .WithId(1)
                .Build(); 

            _updateValidator
                .Setup(x => x.ValidateAsync(
                    request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _tripRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Trip, bool>>>()))
                .ReturnsAsync(false);

            _tripRepository
                .Setup(r => r.GetByIdAsync(trip.Id))
                .ReturnsAsync(trip);

            // Act
            await _tripService.UpdateAsync(trip.Id, request);

            // Assert
            trip.ActualCashCollected.Should().Be(500);
            _tripRepository
                .Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_Request_Is_invalid()
        {
            var request = new TripUpdateRequestBuilder()
                .WithTripNumber(0)
                .Build();

            var validationResult = new ValidationResult(
                new[]
                {
                    new ValidationFailure("TripNumber", "Should be greater than 0")
                });

            _updateValidator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            Func<Task> act = () => _tripService.UpdateAsync(1, request);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_Employee_NotFound()
        {
            var request = new TripUpdateRequestBuilder()
                .Build();

            _updateValidator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(false);

            Func<Task> act = () => _tripService.UpdateAsync(1, request);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_TripNumber_Already_Exists_In_The_Same_Date()
        {
            var request = new TripUpdateRequestBuilder()
                .WithTripNumber(1)
                .Build();

            _updateValidator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _employeeRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(true);

            _tripRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Trip, bool>>>()))
                .ReturnsAsync(true);

            Func<Task> act = () => _tripService.UpdateAsync(1, request);

            await act.Should().ThrowAsync<ConflictException>();
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_Should_Remove_Trip_And_Save()
        {
            var trip = new TripBuilder()
                .WithId(1)
                .Build();

            _tripRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(trip);

            await _tripService.DeleteAsync(1);

            _tripRepository
                .Verify(r => r.Remove(It.IsAny<Trip>()), Times.Once);

            _tripRepository
                .Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_NotFound()
        {
            _tripRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Trip?) null);

            Func<Task> act = () => _tripService.DeleteAsync(1);

            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region GetNextTripNumberAsync
        [Fact]
        public async Task GetNextTripNumberAsync_Should_Increment_Max_Trip_Number()
        {
            DateOnly date = new DateOnly();

            _tripRepository
                .Setup(r => r.GetMaxTripNumberByDateAsync(date))
                .ReturnsAsync(1);

            var result = await _tripService.GetNextTripNumberAsync(date);

            result.Should().NotBeNull();
            result.Date.Should().Be(date);
            result.NextTripNo.Should().Be(2);

            _tripRepository
                .Verify(r => r.GetMaxTripNumberByDateAsync(date), Times.Once);
        }
        #endregion

        #region SearchByDateRangeAsync
        [Fact]
        public async Task SearchByDateRangeAsync_Should_Pass_Correct_Options_To_Repository()
        {
            var request = new DateRangeRequestBuilder()
                .Build();

            _tripRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync([]);

            // Act
            await _tripService.SearchByDateRangeAsync(request);

            // Assert
            _tripRepository.Verify(r => 
                r.SearchByDateRangeAsync(
                    It.Is<DateRangeOptions>(o =>
                        o.StartDate == request.StartDate
                        && o.EndDate == request.EndDate
                        && o.Page == request.Page)),
                Times.Once);
        }

        [Fact]
        public async Task SearchByDateRangeAsync_Should_Map_Trips_To_Response()
        {
            var trip = new TripBuilder()
                .WithTripNumber(10)
                .WithEmployee(new EmployeeBuilder().Build())
                .Build();

            _tripRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync([trip]);

            // Act
            var result = await _tripService.SearchByDateRangeAsync(new DateRangeRequest());

            // Assert
            result.Should().HaveCount(1);
            result[0].TripNumber.Should().Be(10);
            result[0].EmployeeName.Should().Be(trip.Employee.FullName);
        }

        [Fact]
        public async Task SearchByDateRangeAsync_Should_Return_Empty_List_When_No_Trips_Found()
        {
            _tripRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync([]);

            // Act
            var result = await _tripService.SearchByDateRangeAsync(new DateRangeRequest());

            // Assert
            result.Should().BeEmpty();
        }
        #endregion
    }
}
