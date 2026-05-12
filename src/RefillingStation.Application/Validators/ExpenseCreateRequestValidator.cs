using FluentValidation;
using RefillingStation.Application.DTOs.Expenses;

namespace RefillingStation.Api.Features.Expenses.Validators
{
    public class ExpenseCreateRequestValidator : AbstractValidator<ExpenseCreateRequest>
    {
        public ExpenseCreateRequestValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty()
                .Must(date => date.Date <= DateTime.Today)
                .WithMessage("Cannot select future date.");

            RuleFor(x => x.ExpenseCategoryId)
                .NotEmpty();

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.Notes)
                .MaximumLength(500);
        }
    }
}