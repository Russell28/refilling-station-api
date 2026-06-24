using RefillingStation.Application.DTOs.Expenses;

namespace RefillingStation.Tests.Services.Builders
{
    public class ExpenseCreateRequestBuilder
    {
        private DateOnly _date = DateOnly.FromDateTime(DateTime.Now);
        private int _expenseCategoryId = 1;
        private decimal _amount = 100m;
        private string? _notes = null;

        public ExpenseCreateRequestBuilder WithDate(DateOnly date)
        {
            _date = date;
            return this;
        }

        public ExpenseCreateRequestBuilder WithExpenseCategoryId(int expenseCategoryId)
        {
            _expenseCategoryId = expenseCategoryId;
            return this;
        }

        public ExpenseCreateRequestBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public ExpenseCreateRequestBuilder WithNotes(string? notes)
        {
            _notes = notes;
            return this;
        }

        public ExpenseCreateRequest Build()
        {
            var request = new ExpenseCreateRequest
            {
                Date = _date,
                ExpenseCategoryId = _expenseCategoryId,
                Amount = _amount,
                Notes = _notes
            };

            return request;
        }
    }
}
