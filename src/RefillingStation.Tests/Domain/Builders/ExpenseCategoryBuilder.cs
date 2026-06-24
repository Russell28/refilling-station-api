using RefillingStation.Domain.Entities;

namespace RefillingStation.Tests.Domain.Builders
{
    public class ExpenseCategoryBuilder
    {
        private int _id = 1;
        private string _name = "Test Category";
        private int _sortOrder = 1;
        private string? _description = null;
        private ICollection<Expense> _expenses = [];

        public ExpenseCategoryBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public ExpenseCategoryBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ExpenseCategoryBuilder WithSortOrder(int sortOrder)
        {
            _sortOrder = sortOrder;
            return this;
        }

        public ExpenseCategoryBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public ExpenseCategoryBuilder WithExpenses(ICollection<Expense> expenses)
        {
            _expenses = expenses;
            return this;
        }

        public ExpenseCategory Build()
        {
            var category = new ExpenseCategory
            {
                Id = _id,
                Name = _name,
                SortOrder = _sortOrder,
                Description = _description,
                Expenses = _expenses
            };

            return category;
        }
    }
}
