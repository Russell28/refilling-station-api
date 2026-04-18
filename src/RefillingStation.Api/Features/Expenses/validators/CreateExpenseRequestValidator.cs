using FluentValidation;
using RefillingStation.Api.Features.Expenses.dtos;

namespace RefillingStation.Api.Features.Expenses.validators
{
    public class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
    {
        public CreateExpenseRequestValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty()
                .Must(date => date.Date <= DateTime.Today)
                .WithMessage("Cannot select future date.");

            RuleFor(x => x.ExpenseCategory)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }
}