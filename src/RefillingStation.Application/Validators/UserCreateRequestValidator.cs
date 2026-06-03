using FluentValidation;
using RefillingStation.Application.DTOs.Users;

namespace RefillingStation.Application.Validators
{
    public class UserCreateRequestValidator : AbstractValidator<UserCreateRequest>
    {
        public UserCreateRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required.")
                .MaximumLength(20)
                .WithMessage("Username cannot exceed 20 characters.")
                .MinimumLength(5)
                .WithMessage("Username must be at least 5 characters.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MaximumLength(50)
                .WithMessage("Password cannot exceed 50 characters.")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role is required.");
        }
    }
}
