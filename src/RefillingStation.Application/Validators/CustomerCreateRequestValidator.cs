using FluentValidation;
using RefillingStation.Application.DTOs.Customers;

namespace RefillingStation.Api.Features.Customers.Validators
{
    public class CustomerCreateRequestValidator : AbstractValidator<CustomerCreateRequest>
    {
        public CustomerCreateRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Customer name is required.")
                .MaximumLength(100)
                .WithMessage("Customer name cannot exceed 100 characters.");
        }
    }
}
