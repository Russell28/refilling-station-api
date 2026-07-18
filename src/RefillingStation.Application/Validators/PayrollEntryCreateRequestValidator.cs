using FluentValidation;
using RefillingStation.Application.DTOs.PayrollEntries;

namespace RefillingStation.Application.Validators
{
    public class PayrollEntryCreateRequestValidator : AbstractValidator<PayrollEntryCreateRequest>
    {
        public PayrollEntryCreateRequestValidator()
        {
            RuleFor(x => x.EarnedDate)
                .NotEmpty();

            RuleFor(x => x.EmployeeId)
                .NotEmpty();

            RuleFor(x => x.SalaryAmount)
                .GreaterThanOrEqualTo(0);
        }
    }
}
