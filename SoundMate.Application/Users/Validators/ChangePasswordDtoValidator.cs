using FluentValidation;
using SoundMate.Application.Users.DTO;

namespace SoundMate.Application.Users.Validators;

public sealed class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        // Only NotEmpty. The current password is checked against the stored hash, not against
        // today's rules: applying the length rule here would reject anyone whose password predates
        // a tightening of it, and leak that fact in the process.
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("The current password is required.");

        RuleFor(x => x.NewPassword).Password();

        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("The new password must be different from the current one.");
    }
}
