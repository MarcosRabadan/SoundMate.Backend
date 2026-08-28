namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// The user exists but does not study that discipline.
/// <para>
/// Distinct from <see cref="DisciplineNotFoundException"/>, which says the discipline is not in
/// the catalogue at all: one means "you have not added this yet", the other "there is no such
/// thing". A caller that cannot tell them apart does not know whether to offer an <c>Add</c>
/// button or to fix its selector.
/// </para>
/// </summary>
public sealed class StudiedDisciplineNotFoundException : Exception
{
    public StudiedDisciplineNotFoundException(Guid userId, Guid disciplineId)
        : base($"User '{userId}' does not study discipline '{disciplineId}'.")
    {
        UserId = userId;
        DisciplineId = disciplineId;
    }

    public Guid UserId { get; }

    public Guid DisciplineId { get; }
}
