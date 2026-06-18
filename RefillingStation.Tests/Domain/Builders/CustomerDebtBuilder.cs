using RefillingStation.Domain.Entities;


namespace RefillingStation.Tests.Domain.Builders
{
    public class CustomerDebtBuilder
    {
        private CustomerDebtEntry _debt =
            new CustomerDebtEntry
            {
                Id = 1,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Amount = 100
            };

        public CustomerDebtBuilder WithAmount(decimal amount)
        {
            _debt.Amount = amount;
            return this;
        }

        public CustomerDebtBuilder WithCustomer(Customer customer)
        {
            _debt.Customer = customer;
            return this;
        }

        public CustomerDebtEntry Build()
        {
            return _debt;
        }
    }
}
