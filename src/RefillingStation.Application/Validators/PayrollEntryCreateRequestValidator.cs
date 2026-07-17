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

            RuleFor(x => x.CashPaid)
                .GreaterThanOrEqualTo(0);

            //RuleFor(x => x)
            //    .Must(x => x.SalaryAmount > 0
            //            || x.CashPaid > 0)
            //    .WithMessage("A payroll requires at least one of Salary Amount or Cash Paid to be greater than zero.");
        }
    }
}
