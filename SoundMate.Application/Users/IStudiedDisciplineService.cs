using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Users.DTO;

namespace SoundMate.Application.Users;

/// <summary>
/// What a person studies, and at what level — "Pepito plays piano at Advanced".
/// <para>
/// A collection rather than the singleton the profile is, and addressed by the <b>catalogue</b>
/// id: the row's own <c>StudiedDisciplineId</c> never leaves the database, because the id a caller
/// holds is the one they just picked from <c>GET /api/disciplines</c>.
/// </para>
/// <para>
/// Not to be confused with what somebody <b>teaches</b> (<c>TaughtDiscipline</c>), which is global
/// to the person and carries no level.
/// </para>
/// <para>
/// A soft-deleted user has no reachable disciplines: every operation here answers
/// <see cref="UserNotFoundException"/> for one, exactly as the rest of the user surface does.
/// </para>
/// </summary>
public interface IStudiedDisciplineService
{
    /// <summary>
    /// Everything that person studies, each with the catalogue name and family resolved.
    /// <para>
    /// Disciplines retired from the catalogue are still listed. Somebody having reached Advanced
    /// on the bandurria stays true after you stop offering it.
    /// </para>
    /// </summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    Task<IReadOnlyList<StudiedDisciplineDto>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Takes up a discipline at a level.</summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    /// <exception cref="DisciplineNotFoundException">That id is not in the catalogue.</exception>
    /// <exception cref="DisciplineNotAvailableException">The discipline has been retired from the catalogue.</exception>
    /// <exception cref="DisciplineAlreadyAddedException">They already study it — change the level instead.</exception>
    Task<StudiedDisciplineDto> AddAsync(Guid userId, AddStudiedDisciplineDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves an already-studied discipline to another level.
    /// <para>
    /// Works on retired disciplines too: refusing would freeze the level of anybody caught by a
    /// catalogue change they had nothing to do with.
    /// </para>
    /// </summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    /// <exception cref="StudiedDisciplineNotFoundException">They do not study it. Add it first.</exception>
    /// <exception cref="Domain.Common.DomainException">The level is not a defined value.</exception>
    Task<StudiedDisciplineDto> ChangeLevelAsync(Guid userId,
                                                Guid disciplineId,
                                                ChangeLevelDto dto,
                                                CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops studying it. A plain delete: nothing references a <c>StudiedDisciplineId</c>, so
    /// there is nothing to orphan, and this is a claim about oneself rather than identity.
    /// </summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    /// <exception cref="StudiedDisciplineNotFoundException">They do not study it.</exception>
    Task RemoveAsync(Guid userId, Guid disciplineId, CancellationToken cancellationToken = default);
}
