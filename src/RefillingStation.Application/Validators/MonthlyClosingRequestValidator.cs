using FluentValidation;
using RefillingStation.Application.DTOs.MonthlyClosing;

namespace RefillingStation.Application.Validators
{
    public class MonthlyClosingRequestValidator : AbstractValidator<MonthlyClosingRequest>
    {
        public MonthlyClosingRequestValidator()
        {
            RuleFor(x => x.MonthYear)
            .NotEmpty().WithMessage("MonthYear is required.")
            .Matches(@"^\d{4}-(0[1-9]|1[0-2])$")
            .WithMessage("MonthYear must be in yyyy-MM format.");

            RuleFor(x => x.ManagerShare)
                .GreaterThanOrEqualTo(0).WithMessage("Manager share cannot be negative.");

            RuleFor(x => x.OwnerShare)
                .GreaterThanOrEqualTo(0).WithMessage("Owner share cannot be negative.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
        }
    }
}
