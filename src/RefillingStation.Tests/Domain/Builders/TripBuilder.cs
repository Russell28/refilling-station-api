using RefillingStation.Domain.Entities;
using System.Security.Cryptography;

namespace RefillingStation.Tests.Domain.Builders
{
    public class TripBuilder
    {
        private Trip _trip = new Trip
        {
            Date = new DateOnly(),
            TripNumber = 1,
            EmployeeId = 1,
            ActualCashCollected = 1
        };

        public TripBuilder WithId(int id)
        {
            _trip.Id = id;
            return this;
        }

        public TripBuilder WithDate(DateOnly date)
        {
            _trip.Date = date;
            return this;
        }

        public TripBuilder WithTripNumber(int tripNumber)
        {
            _trip.TripNumber = tripNumber;
            return this;
        }

        public TripBuilder WithEmployee(Employee employee)
        {
            _trip.Employee = employee;
            return this;
        }

        public TripBuilder WithActualCashCollected(decimal actualCash)
        {
            _trip.ActualCashCollected = actualCash;
            return this;
        }

        public Trip Build()
        {
            return _trip;
        }
    }
}
