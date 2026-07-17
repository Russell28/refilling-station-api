using FluentValidation;
using RefillingStation.Application.DTOs.PayrollPayments;

namespace RefillingStation.Application.Validators
{
    public class PayrollPaymentCreateRequestValidator : AbstractValidator<PayrollPaymentCreateRequest>
    {
        public PayrollPaymentCreateRequestValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty();

            RuleFor(x => x.PaidDate)
                .NotEmpty();

            RuleFor(x => x.AmountPaid)
                .NotEmpty()
                .GreaterThan(0);
        }
    }
}
