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
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("Email is required.");

        var value = input.Trim();

        if (value.Length > MaxLength)
            throw new DomainException($"Email cannot exceed {MaxLength} characters.");

        if (!EmailRegex().IsMatch(value))
            throw new DomainException($"Email '{input}' is not a valid format.");

        return new Email(value);
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
