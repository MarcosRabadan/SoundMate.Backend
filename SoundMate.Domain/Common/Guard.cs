using System.Numerics;

namespace SoundMate.Domain.Common;

/// <summary>
/// Guard clauses for enforcing invariants in factories and behavior methods. Each throws a
/// <see cref="DomainException"/> so an aggregate can never be built or left in an invalid
/// state — errors surface at construction (fail-fast), not later at SaveChanges.
/// </summary>
public static class Guard
{
    /// <summary>Requires a non-empty string; returns it trimmed.</summary>
    public static string NotNullOrWhiteSpace(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{field} is required.");

        return value.Trim();
    }

    /// <summary>Requires a strongly-typed id that is not the default (empty) value.</summary>
    public static TId NotEmpty<TId>(TId id, string field) where TId : struct
    {
        if (EqualityComparer<TId>.Default.Equals(id, default))
            throw new DomainException($"{field} is required.");

        return id;
    }

    /// <summary>Requires an enum value that is actually defined.</summary>
    public static TEnum Defined<TEnum>(TEnum value, string field) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new DomainException($"{field} '{value}' is not a valid value.");

        return value;
    }

    /// <summary>Requires a number within an inclusive range.</summary>
    public static T InRange<T>(T value, T min, T max, string field) where T : INumber<T>
    {
        if (value < min || value > max)
            throw new DomainException($"{field} must be between {min} and {max}.");

        return value;
    }
}
