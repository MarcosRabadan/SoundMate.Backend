namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// Refuses to purge an academy that still has members.
/// <para>
/// Two aggregates carry an <c>AcademyId</c> with <b>no enforced foreign key</b> — deliberate, so a
/// future database-per-service split stays cheap: <c>Membership</c> and <c>TeacherReview</c>.
/// Nothing at the database level stops a delete from orphaning either. Memberships are the anchor,
/// so they are the check; the reviews are why a purge still needs a real cascade first.
/// </para>
/// </summary>
public sealed class AcademyStillHasMembersException : Exception
{
    public AcademyStillHasMembersException(Guid id, int count)
        : base($"Academy '{id}' still has {count} member(s). Have them leave first, or delete the " +
               "academy instead of purging it.")
    {
        Id = id;
        MemberCount = count;
    }

    public Guid Id { get; }

    public int MemberCount { get; }
}
