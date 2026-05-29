using FluentValidation;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.Payrolls;
using RefillingStation.Application.DTOs.Trips;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class TripService : ITripService
    {
        private readonly ITripRepository _tripRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IValidator<TripCreateRequest> _createValidator;
        private readonly IValidator<TripUpdateRequest> _updateValidator;

        public TripService(
            ITripRepository repository,
            IEmployeeRepository employeeRepository,
            IValidator<TripCreateRequest> validator,
            IValidator<TripUpdateRequest> updateValidator)
        {
            _tripRepository = repository;
            _employeeRepository = employeeRepository;
            _createValidator = validator;
            _updateValidator = updateValidator;
        }
        public async Task<List<TripDetailResponse>> GetAllAsync()
        {
            var trips = await _tripRepository.GetAllAsync();

            return trips
                .Select(x => new TripDetailResponse
                (
                    x.Id,
                    x.Date,
                    x.TripNumber,
                    x.EmployeeId,
                    x.Employee.FullName,
                    x.CustomerCategory,
                    x.CollectedQty,
                    x.LoadedQty,
                    x.DeliveredQty,
                    x.FreeQty,
                    x.ReturnedQty,
                    x.ReplacementQty,
                    x.ActualCashCollected,
                    x.IsRemitted,
                    x.Notes
                ))
                .ToList();
        }

        public async Task<TripDetailResponse> GetByIdAsync(int id)
        {
            var trip = await _tripRepository.GetByIdAsync(id);

            if (trip is null)
                throw new NotFoundException("Trip", id);

            return new TripDetailResponse(
                trip.Id,
                trip.Date,
                trip.TripNumber,
                trip.EmployeeId,
                trip.Employee.FullName,
                trip.CustomerCategory,
                trip.CollectedQty,
                trip.LoadedQty,
                trip.DeliveredQty,
                trip.FreeQty,
                trip.ReturnedQty,
                trip.ReplacementQty,
                trip.ActualCashCollected,
                trip.IsRemitted,
                trip.Notes
            );
        }

        public async Task<int> CreateAsync(TripCreateRequest request)
        {
            var validation = await _createValidator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var employeeExists = await _employeeRepository.ExistsAsync(e => e.Id == request.EmployeeId);

            if (!employeeExists)
                throw new NotFoundException("Employee", request.EmployeeId);

            // Check for duplicate
            var tripNoExists = await _tripRepository.ExistsAsync(x =>
                x.Date == request.Date
                && x.TripNumber == request.TripNumber);

            if (tripNoExists)
                throw new ConflictException($"Trip number already exists for this date.");

            var trip = new Trip
            {
                Date = request.Date,
                TripNumber = request.TripNumber,
                TimeStarted = request.TimeStarted,
                TimeEnded = request.TimeEnded,
                EmployeeId = request.EmployeeId,
                Source = request.Source,
                TripType = request.TripType,
                CustomerCategory = request.CustomerCategory,
                CollectedQty = request.CollectedQty,
                LoadedQty = request.LoadedQty,
                DeliveredQty = request.DeliveredQty,
                FreeQty = request.FreeQty,
                ReturnedQty = request.ReturnedQty,
                ReplacementQty = request.ReplacementQty,
                ActualCashCollected = request.ActualCashCollected,
                Notes = request.Notes
            };

            await _tripRepository.AddAsync(trip);
            await _tripRepository.SaveChangesAsync();

            return trip.Id;
        }

        public async Task UpdateAsync(int id, TripUpdateRequest request)
        {
            var validation = await _updateValidator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var employeeExists = await _employeeRepository.ExistsAsync(e => e.Id == request.EmployeeId);

            if (!employeeExists)
                throw new NotFoundException("Employee", request.EmployeeId);

            // Check for duplicate
            var tripNoExists = await _tripRepository.ExistsAsync(x =>
                x.Id != id // exclude self
                && x.Date == request.Date
                && x.TripNumber == request.TripNumber);

            if (tripNoExists)
                throw new ConflictException("Trip number already exists for this date.");

            var trip = await _tripRepository.GetByIdAsync(id);

            if (trip is null)
                throw new NotFoundException("Trip", id);

            trip.Date = request.Date;
            trip.TripNumber = request.TripNumber;
            trip.TimeStarted = request.TimeStarted;
            trip.TimeEnded = request.TimeEnded;
            trip.EmployeeId = request.EmployeeId;
            trip.Source = request.Source;
            trip.TripType = request.TripType;
            trip.CustomerCategory = request.CustomerCategory;
            trip.CollectedQty = request.CollectedQty;
            trip.LoadedQty = request.LoadedQty;
            trip.DeliveredQty = request.DeliveredQty;
            trip.FreeQty = request.FreeQty;
            trip.ReturnedQty = request.ReturnedQty;
            trip.ReplacementQty = request.ReplacementQty;
            trip.ActualCashCollected = request.ActualCashCollected;
            trip.IsRemitted = request.IsRemitted;
            trip.Notes = request.Notes;

            await _tripRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var trip = await _tripRepository.GetByIdAsync(id);

            if (trip is null)
                throw new NotFoundException("Trip", id);

            _tripRepository.Remove(trip);

            await _tripRepository.SaveChangesAsync();
        }

        public async Task<NextTripNumberResponse> GetNextTripNumberAsync(DateOnly date)
        {
            var max = await _tripRepository.GetMaxTripNumberByDateAsync(date);
            var nextTripNo = (max ?? 0) + 1; // if max == null (0) + 1

            return new NextTripNumberResponse(date, nextTripNo);
        }

        public async Task<List<TripDetailResponse>> SearchByDateRangeAsync(DateRangeRequest request)
        {
            var options = new DateRangeOptions
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Page = request.Page
            };

            var trips = await _tripRepository.SearchByDateRangeAsync(options);

            return trips
                .Select(x => new TripDetailResponse
                (
                    x.Id,
                    x.Date,
                    x.TripNumber,
                    x.EmployeeId,
                    x.Employee.FullName,
                    x.CustomerCategory,
                    x.CollectedQty,
                    x.LoadedQty,
                    x.DeliveredQty,
                    x.FreeQty,
                    x.ReturnedQty,
                    x.ReplacementQty,
                    x.ActualCashCollected,
                    x.IsRemitted,
                    x.Notes
                ))
                .ToList();
        }
    }
}
