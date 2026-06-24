using RefillingStation.Application.DTOs.CustomerDebts;

namespace RefillingStation.Tests.Services.Builders
{
    public class CustomerDebtCreateRequestBuilder
    {
        private DateOnly _date = new DateOnly();
        private int _customerId = 1;
        private decimal _amount = 0;

        public CustomerDebtCreateRequestBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public CustomerDebtCreateRequestBuilder WithDate(DateOnly date)
        {
            _date = date;
            return this;
        }

        public CustomerDebtCreateRequest Build()
        {
            var request =
                new CustomerDebtCreateRequest
                {
                    Date = _date,
                    Amount = _amount,
                    CustomerId = _customerId
                };

            return request;
        }
    }
}
