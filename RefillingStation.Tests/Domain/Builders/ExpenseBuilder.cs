using RefillingStation.Domain.Entities;

namespace RefillingStation.Tests.Domain.Builders
{
    public class ExpenseBuilder
    {
        private int _id = 1;
        private DateOnly _date = DateOnly.FromDateTime(DateTime.Now);
        private int _expenseCategoryId = 1;
        private decimal _amount = 100m;
        private string? _notes = null;
        private ExpenseCategory _category = new ExpenseCategoryBuilder().Build();

        public ExpenseBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public ExpenseBuilder WithDate(DateOnly date)
        {
            _date = date;
            return this;
        }

        public ExpenseBuilder WithExpenseCategoryId(int expenseCategoryId)
        {
            _expenseCategoryId = expenseCategoryId;
            return this;
        }

        public ExpenseBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public ExpenseBuilder WithNotes(string? notes)
        {
            _notes = notes;
            return this;
        }

        public ExpenseBuilder WithCategory(ExpenseCategory category)
        {
            _category = category;
            _expenseCategoryId = category.Id;
            return this;
        }

        public Expense Build()
        {
            var expense = new Expense
            {
                Id = _id,
                Date = _date,
                ExpenseCategoryId = _expenseCategoryId,
                Amount = _amount,
                Notes = _notes,
                Category = _category
            };

            return expense;
        }
    }
}
