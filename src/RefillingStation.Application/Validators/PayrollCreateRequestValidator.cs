using FluentValidation;
using RefillingStation.Application.DTOs.Payrolls;

namespace RefillingStation.Api.Features.Payrolls.Validators
{
    public class PayrollCreateRequestValidator : AbstractValidator<PayrollCreateRequest>
    {
        public PayrollCreateRequestValidator()
        {
            RuleFor(x => x.EarnedDate)
                .NotEmpty();

            RuleFor(x => x.EmployeeId)
                .NotEmpty();

            RuleFor(x => x.SalaryAmount)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.CashPaid)
                .GreaterThanOrEqualTo(0);

            //RuleFor(x => x)
            //    .Must(x => x.SalaryAmount > 0
            //            || x.CashPaid > 0)
            //    .WithMessage("A payroll requires at least one of Salary Amount or Cash Paid to be greater than zero.");
        }
    }
}
