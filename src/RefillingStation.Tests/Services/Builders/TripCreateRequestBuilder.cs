using RefillingStation.Application.DTOs.Trips;

namespace RefillingStation.Tests.Services.Builders
{
    public class TripCreateRequestBuilder
    {
        private TripCreateRequest _request = new TripCreateRequest
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
            Notes = ""
        };

        public TripCreateRequestBuilder WithTripNumber(int number)
        {
            _request.TripNumber = number;
            return this;
        }

        public TripCreateRequestBuilder WithEmployeeId(int id)
        {
            _request.EmployeeId = id;
            return this;
        }

        public TripCreateRequestBuilder WithActualCashCollected(decimal actualCashCollected)
        {
            _request.ActualCashCollected = actualCashCollected;
            return this;
        }

        public TripCreateRequest Build() => _request;
    }
}
