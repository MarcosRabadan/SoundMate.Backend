using FluentValidation;
using SoundMate.Domain.Academies;

namespace SoundMate.Application.Academies.Validators;

/// <summary>
/// The rules shared by every validator that touches an academy, so creating and editing cannot
/// drift into disagreeing about what a valid name or slug is — the failure mode that let
/// <c>missing@domain</c> through on the user side.
/// <para>Each one uses <c>CascadeMode.Stop</c> so a field reports one reason, not a list.</para>
/// </summary>
internal static class AcademyRuleExtensions
{
    /// <summary>Mirrors the column width in <c>AcademyConfiguration</c>.</summary>
    public const int MaxNameLength = 200;

    public static IRuleBuilderOptions<T, string> AcademyName<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Academy name is required.")
            .MaximumLength(MaxNameLength);

    /// <summary>
    /// Delegates the format to <see cref="Slug.IsValid"/> rather than repeating the pattern here.
    /// A validator that restates a domain rule in its own words eventually disagrees with it, and
    /// the disagreement surfaces as input that passes the 400 and then throws an invariant.
    /// </summary>
    public static IRuleBuilderOptions<T, string> AcademySlug<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(Slug.MaxLength)
            .Must(Slug.IsValid)
                .WithMessage("Slug only allows lowercase letters, digits and single hyphens between them.");
}
