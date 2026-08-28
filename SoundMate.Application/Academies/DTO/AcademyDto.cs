using SoundMate.Domain.Academies;

namespace SoundMate.Application.Academies.DTO;

/// <summary>
/// An academy as the API hands it back.
/// <para>
/// Every member is <c>required</c> because <c>AcademyMapper</c> is the only thing that builds one:
/// a forgotten field is <c>error CS9035</c> at build time rather than a silent default on the
/// wire.
/// </para>
/// <para>
/// <c>DeletedAtUtc</c> is deliberately absent. A deleted academy never reaches a response, so the
/// field would be null in every one of them — a column of nulls that invites someone to start
/// relying on it.
/// </para>
/// </summary>
public sealed record AcademyDto
{
    /// <summary>The academy's identifier, unwrapped from <c>AcademyId</c>.</summary>
    public required Guid Id { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary><c>Academy</c> for an organisation, <c>SoloTeacher</c> for a private teacher.</summary>
    public required AcademyType Type { get; init; }

    /// <summary>The public handle, unwrapped from the <c>Slug</c> value object.</summary>
    public required string Slug { get; init; }

    /// <summary>The owner's id, unwrapped from <c>UserId</c>.</summary>
    public required Guid OwnerUserId { get; init; }

    /// <summary>Which subscription tier it is on.</summary>
    public required SubscriptionPlan Plan { get; init; }

    /// <summary>
    /// Where the business stands: running, suspended, or closed.
    /// <para>
    /// The enums here are real enums, not strings, and they still go on the wire as names —
    /// <c>"SoloTeacher"</c>, not <c>2</c> — because <c>JsonStringEnumConverter</c> is registered
    /// globally in <c>Program.cs</c>. So the HTTP contract stays independent of the numeric
    /// values (a storage detail) while OpenAPI documents the allowed set and C# consumers get the
    /// type. Mapping by hand to a string would have thrown all three away for nothing.
    /// </para>
    /// </summary>
    public required AcademyStatus Status { get; init; }

    /// <summary>When the academy was opened. UTC, like every instant in SoundMate.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}
