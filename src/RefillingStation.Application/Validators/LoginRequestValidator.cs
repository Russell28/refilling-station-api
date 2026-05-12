using FluentValidation;
using RefillingStation.Application.DTOs.Auth;

namespace RefillingStation.Application.Validators
{
    internal class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator() 
        { 
            RuleFor(x => x.Username)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MaximumLength(50);
        }
    }
}
