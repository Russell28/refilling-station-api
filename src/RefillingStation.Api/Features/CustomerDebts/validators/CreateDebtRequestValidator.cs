using FluentValidation;
using RefillingStation.Application.DTOs.Debts;

namespace RefillingStation.Api.Features.CustomerDebts.validators
{
    public class CreateDebtRequestValidator : AbstractValidator<CreateDebtRequest>
    {
        public CreateDebtRequestValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty()
                .Must(date => date.Date <= DateTime.Today)
                .WithMessage("Cannot select future date.");

            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required.");

            RuleFor(x => x.Amount)
                .NotEqual(0).WithMessage("Amount cannot be zero.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
        }
    }
}
