using RefillingStation.Application.DTOs.Trips;

namespace RefillingStation.Tests.Services.Builders
{
    public class TripUpdateRequestBuilder
    {
        private TripUpdateRequest _request = new TripUpdateRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TripNumber = 1,
            TimeStarted = DateTime.UtcNow,
            TimeEnded = DateTime.UtcNow.AddMinutes(60),
            EmployeeId = 1,
            Source = "",
            TripType = "",
            CustomerCategory = "Mixed",
            CollectedQty = 1,
            LoadedQty = 0,
            DeliveredQty = 1,
            FreeQty = 0,
            ReturnedQty = 0,
            ReplacementQty = 0,
            ActualCashCollected = 25,
            IsRemitted = false,
            Notes = ""
        };

        public TripUpdateRequestBuilder WithTripNumber(int number)
        {
            _request.TripNumber = number;
            return this;
        }

        public TripUpdateRequestBuilder WithEmployeeId(int id)
        {
            _request.EmployeeId = id;
            return this;
        }

        public TripUpdateRequestBuilder WithActualCashCollected(decimal actualCashCollected)
        {
            _request.ActualCashCollected = actualCashCollected;
            return this;
        }

        public TripUpdateRequest Build() => _request;
    }
}
