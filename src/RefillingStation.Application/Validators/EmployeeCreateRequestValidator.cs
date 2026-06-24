using FluentValidation;
using RefillingStation.Application.DTOs.Employees;

namespace RefillingStation.Application.Validators
{
    public class EmployeeCreateRequestValidator : AbstractValidator<EmployeeCreateRequest>
    {
        public EmployeeCreateRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First Name is required.")
                .MaximumLength(20)
                .WithMessage("First Name cannot exceed 20 characters.")
                .MinimumLength(3)
                .WithMessage("First Name must be at least 3 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last Name is required.")
                .MaximumLength(20)
                .WithMessage("Last Name cannot exceed 20 characters.")
                .MinimumLength(3)
                .WithMessage("Last Name must be at least 3 characters.");

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role is required.")
                .IsInEnum()
                .WithMessage("Role must be a valid value.");

            RuleFor(x => x.EmploymentType)
                .NotNull()
                .WithMessage("Employee type is required.")
                .IsInEnum()
                .WithMessage("Employee type must be a valid value.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone Number is required.")
                .Matches(@"^\+639\d{9}$")
                .WithMessage("Phone Number must be in the format +639XXXXXXXXX.");
        }
    }
}
