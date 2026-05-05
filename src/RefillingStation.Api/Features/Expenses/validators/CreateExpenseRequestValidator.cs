using FluentValidation;
using RefillingStation.Application.DTOs.Expenses;

namespace RefillingStation.Api.Features.Expenses.validators
{
    public class CreateExpenseRequestValidator : AbstractValidator<ExpenseCreateRequest>
    {
        public CreateExpenseRequestValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty()
                .Must(date => date.Date <= DateTime.Today)
                .WithMessage("Cannot select future date.");

            RuleFor(x => x.ExpenseCategoryId)
                .NotEmpty();

            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }
}