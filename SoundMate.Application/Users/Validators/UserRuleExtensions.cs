using FluentValidation;

namespace SoundMate.Application.Users.Validators;

/// <summary>
/// The shared rules themselves, not just the numbers behind them.
/// <para>
/// Registering and updating check the same fields, and a rule copy-pasted between two validators
/// is a rule that eventually disagrees with itself — the exact failure mode that let
/// <c>missing@domain</c> through. One definition, called from both.
/// </para>
/// <para>
/// Each one uses <c>CascadeMode.Stop</c> so a single field reports a single reason: telling a
/// caller their password is both "required" and "too short" is noise.
/// </para>
/// </summary>
internal static class UserRuleExtensions
{
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(UserRules.MinPasswordLength)
                .WithMessage($"Password must be at least {UserRules.MinPasswordLength} characters.")
            .MaximumLength(UserRules.MaxPasswordLength);

    public static IRuleBuilderOptions<T, string> FullName<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(UserRules.MaxFullNameLength);

    /// <summary>
    /// No <c>NotEmpty</c>: the phone is optional, and FluentValidation's length rules already pass
    /// on null, so sending <c>null</c> to clear it is legal.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> Phone<T>(this IRuleBuilderInitial<T, string?> rule) =>
        rule.MaximumLength(UserRules.MaxPhoneLength);
}
