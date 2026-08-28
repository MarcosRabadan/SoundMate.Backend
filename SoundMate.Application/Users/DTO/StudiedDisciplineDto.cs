using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Users.DTO;

/// <summary>
/// One discipline a person studies, with the level and the catalogue entry already resolved.
/// <para>
/// It carries the <b>discipline's</b> id, never the row's. A <c>StudiedDisciplineId</c> exists in
/// the database and stays there: the id the caller holds is the one they just picked in a
/// selector, so that is what addresses the sub-resource. Same criterion as the profile of #11.
/// </para>
/// <para>
/// <see cref="Name"/> and <see cref="Category"/> are duplicated from the catalogue so a screen can
/// render this without fetching all 48 disciplines and joining by hand. The usual objection to
/// copying a value — that the copies drift apart — does not apply: this is seeded reference data
/// that does not get renamed, and the copy is made per response, not stored.
/// </para>
/// </summary>
public sealed record StudiedDisciplineDto
{
    /// <summary>The catalogue id. This is what addresses the row in <c>PUT</c> and <c>DELETE</c>.</summary>
    public required Guid DisciplineId { get; init; }

    /// <summary>"Piano", "Classical guitar"... resolved from the catalogue.</summary>
    public required string Name { get; init; }

    /// <summary>The family the discipline belongs to, resolved from the catalogue.</summary>
    public required DisciplineCategory Category { get; init; }

    /// <summary>How far along this person is on it.</summary>
    public required MusicLevel Level { get; init; }

    /// <summary>
    /// When the row was created — <b>not</b> since when they play. UTC, like every instant here.
    /// </summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Last level change.</summary>
    public required DateTime UpdatedAtUtc { get; init; }
}
