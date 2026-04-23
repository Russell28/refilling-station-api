using FluentValidation;
using RefillingStation.Api.Features.Customers.dtos;

namespace RefillingStation.Api.Features.Customers.validators
{
    public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
    {
        public CreateCustomerRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Customer name is required.")
                .MaximumLength(100).WithMessage("Customer name cannot exceed 100 characters.");
        }
    }
}
