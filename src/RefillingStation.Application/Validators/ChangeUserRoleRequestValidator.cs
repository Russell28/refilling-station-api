using FluentValidation;
using RefillingStation.Application.DTOs.Users;

namespace RefillingStation.Application.Validators
{
    public class ChangeUserRoleRequestValidator : AbstractValidator<ChangeUserRoleRequest>
    {
        public ChangeUserRoleRequestValidator()
        {
            RuleFor(x => x.Role)
                .IsInEnum()
                .WithMessage("Role must be a valid UserRole value.");
        }
    }
}
