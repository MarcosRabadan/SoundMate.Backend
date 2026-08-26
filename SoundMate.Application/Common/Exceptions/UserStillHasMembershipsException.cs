namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// Refuses to delete a user who still belongs to an academy.
/// <para>
/// Aggregates reference each other by identity with <b>no enforced foreign keys</b> — deliberate,
/// so a future database-per-service split stays cheap — which means nothing at the database level
/// stops a delete from orphaning rows. Eight tables carry a <c>UserId</c>, and Agendia holds an
/// <c>Employee</c> pointing at the same person. A <c>Membership</c> is the anchor: while one
/// exists, there is a real relationship to tear down first.
/// </para>
/// </summary>
public sealed class UserStillHasMembershipsException : Exception
{
    public UserStillHasMembershipsException(Guid id, int count)
        : base($"User '{id}' still belongs to {count} academy/academies. Leave them first, or " +
               "suspend the user instead of deleting them.")
    {
        Id = id;
        MembershipCount = count;
    }

    public Guid Id { get; }

    public int MembershipCount { get; }
}
