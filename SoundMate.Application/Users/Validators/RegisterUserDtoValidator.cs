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
    /// <summary>Short enough to be memorable, long enough not to be guessable.</summary>
    public const int MinPasswordLength = 8;

    /// <summary>
    /// PBKDF2 has no practical input limit, but an unbounded password is free CPU for whoever
    /// sends it: every attempt costs us a full key derivation.
    /// </summary>
    public const int MaxPasswordLength = 128;

    // These mirror the column widths in UserConfiguration. Duplicated on purpose: catching it
    // here is a 400 with the offending field, catching it there is a database error.
    private const int MaxFullNameLength = 200;
    private const int MaxPhoneLength = 30;

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

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(MinPasswordLength)
                .WithMessage($"Password must be at least {MinPasswordLength} characters.")
            .MaximumLength(MaxPasswordLength);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(MaxFullNameLength);

        RuleFor(x => x.Phone)
            .MaximumLength(MaxPhoneLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
