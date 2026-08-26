using System.Text.RegularExpressions;
using SoundMate.Domain.Common;

namespace SoundMate.Domain.Users;

public sealed partial class Email : ValueObject
{
    public const int MaxLength = 256;

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string input)
    {
        var error = Validate(input, out var value);

        if (error is not null)
            throw new DomainException(error);

        return new Email(value);
    }

    /// <summary>
    /// True when <see cref="Create"/> would succeed, without paying for an exception.
    /// <para>
    /// It exists so callers that need to *ask* — a request validator wanting to answer with a
    /// per-field message rather than a thrown invariant — use the same rule the aggregate
    /// enforces. Anything that reimplements "looks like an email" drifts from this one, and the
    /// drift shows up as input that passes validation and then fails construction.
    /// </para>
    /// </summary>
    public static bool IsValid(string? input) => Validate(input, out _) is null;

    /// <summary>
    /// The single definition of a well-formed email. Returns null when <paramref name="input"/> is
    /// valid, leaving the normalized value in <paramref name="normalized"/>; otherwise returns the
    /// reason it is not.
    /// </summary>
    private static string? Validate(string? input, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
            return "Email is required.";

        var value = input.Trim();

        if (value.Length > MaxLength)
            return $"Email cannot exceed {MaxLength} characters.";

        if (!EmailRegex().IsMatch(value))
            return $"Email '{input}' is not a valid format.";

        normalized = value;
        return null;
    }

    public string Normalized => Value.ToUpperInvariant();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Normalized;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
