using FluentValidation;
using RefillingStation.Application.DTOs.Expenses;

namespace RefillingStation.Application.Validators
{
    public class ExpenseCreateRequestValidator : AbstractValidator<ExpenseCreateRequest>
    {
        public ExpenseCreateRequestValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty()
                .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
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