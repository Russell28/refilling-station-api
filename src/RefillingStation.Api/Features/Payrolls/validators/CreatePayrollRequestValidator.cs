using FluentValidation;
using RefillingStation.Api.Features.Payrolls.dtos;

namespace RefillingStation.Api.Features.Payrolls.validators
{
    public class CreatePayrollRequestValidator : AbstractValidator<CreatePayrollRequest>
    {
        public CreatePayrollRequestValidator()
        {
            RuleFor(x => x.EarnedDate)
                .NotEmpty()
                .Must(date => date.Date <= DateTime.Today)
                .WithMessage("Cannot select future date.");

            RuleFor(x => x.EmployeeName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.SalaryAmount).GreaterThanOrEqualTo(0);

            RuleFor(x => x.CashPaid).GreaterThanOrEqualTo(0);

            RuleFor(x => x)
                .Must(x => x.SalaryAmount > 0
                        || x.CashPaid > 0)
                .WithMessage("A payroll requires at least one of Salary Amount or Cash Paid to be greater than zero.");
        }
    }
}
