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
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("Slug is required.");

        var value = input.Trim().ToLowerInvariant();

        if (value.Length > MaxLength)
            throw new DomainException($"Slug cannot exceed {MaxLength} characters.");

        if (!SlugRegex().IsMatch(value))
            throw new DomainException($"Slug '{input}' only allows lowercase letters, digits and hyphens.");

        return new Slug(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugRegex();
}
