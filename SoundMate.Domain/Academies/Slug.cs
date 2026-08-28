using System.Text.RegularExpressions;
using SoundMate.Domain.Common;

namespace SoundMate.Domain.Academies;

/// <summary>
/// Human-readable, unique identifier of an academy for use in URLs (e.g. "do-re-mi-academy").
/// Only lowercase letters, digits and hyphens; a hyphen may never be leading, trailing or
/// doubled. It coexists with the numeric <see cref="AcademyId"/>: the id is the internal
/// identity, the slug is the public, pretty, SEO-friendly handle.
/// </summary>
public sealed partial class Slug : ValueObject
{
    public const int MaxLength = 100;

    public string Value { get; }

    private Slug(string value) => Value = value;

    public static Slug Create(string input)
    {
        var error = Validate(input, out var value);

        if (error is not null)
            throw new DomainException(error);

        return new Slug(value);
    }

    /// <summary>
    /// True when <see cref="Create"/> would succeed, without paying for an exception.
    /// <para>
    /// It exists so a request validator can answer with a per-field message instead of letting an
    /// invariant be thrown, <b>using this same rule</b>. Anything that reimplements "looks like a
    /// slug" drifts from this one, and the drift shows up as input that passes validation and then
    /// fails construction — the exact bug <c>Email.IsValid</c> was added to close.
    /// </para>
    /// </summary>
    public static bool IsValid(string? input) => Validate(input, out _) is null;

    /// <summary>
    /// The single definition of a well-formed slug. Returns null when <paramref name="input"/> is
    /// valid, leaving the normalized value in <paramref name="normalized"/>; otherwise returns the
    /// reason it is not.
    /// </summary>
    private static string? Validate(string? input, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
            return "Slug is required.";

        var value = input.Trim().ToLowerInvariant();

        if (value.Length > MaxLength)
            return $"Slug cannot exceed {MaxLength} characters.";

        if (!SlugRegex().IsMatch(value))
            return $"Slug '{input}' only allows lowercase letters, digits and hyphens.";

        normalized = value;
        return null;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugRegex();
}
