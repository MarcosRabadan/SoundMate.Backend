using FluentValidation;
using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Users.Validators;

/// <summary>
/// Shape checks on the way in. The real invariants stay in the domain (<c>Email.Create</c>,
/// <c>User.Register</c>); this exists so a malformed request comes back as a 400 listing every
/// bad field at once, instead of a domain exception that names only the first one it hit.
/// </summary>
public sealed class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        // Email.IsValid, not FluentValidation's EmailAddress(): the built-in one only checks for
        // a single "@" with something either side, so "missing@domain" passes it and is then
        // rejected by Email.Create — the caller would get a thrown invariant instead of the
        // per-field 400 this validator exists to give them. One rule, defined in the domain.
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(Email.MaxLength)
            .Must(Email.IsValid).WithMessage("Email is not a valid format.");

        RuleFor(x => x.Password).Password();

        RuleFor(x => x.FullName).FullName();

        RuleFor(x => x.Phone).Phone();
    }
}
