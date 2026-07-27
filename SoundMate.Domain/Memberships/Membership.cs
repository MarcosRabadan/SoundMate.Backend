using SoundMate.Domain.Academies;
using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Memberships;

/// <summary>
/// A person's membership of an academy, with their role. The "anchor": it exists as soon as
/// someone has any relationship with an academy. Once it has <see cref="MembershipStatus.Left"/>
/// it cannot be modified, and leaving always sets the status and the date together so the two
/// can never disagree.
/// </summary>
public sealed class Membership : AggregateRoot<MembershipId>
{
    public UserId UserId { get; private set; }
    public AcademyId AcademyId { get; private set; }
    public MembershipRole Role { get; private set; }
    public MembershipStatus Status { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }
    public DateTime? LeftAtUtc { get; private set; }

    private Membership() { }

    private Membership(MembershipId id, UserId userId, AcademyId academyId, MembershipRole role) : base(id)
    {
        UserId = userId;
        AcademyId = academyId;
        Role = role;
        Status = MembershipStatus.Active;
        JoinedAtUtc = DateTime.UtcNow;
    }

    public static Membership Create(UserId userId, AcademyId academyId, MembershipRole role)
        => new(
            MembershipId.New(),
            Guard.NotEmpty(userId, "User"),
            Guard.NotEmpty(academyId, "Academy"),
            Guard.Defined(role, "Role"));

    public void ChangeRole(MembershipRole role)
    {
        EnsureNotLeft();
        Role = Guard.Defined(role, "Role");
    }

    public void Pause()
    {
        EnsureNotLeft();
        Status = MembershipStatus.Paused;
    }

    public void Resume()
    {
        EnsureNotLeft();
        Status = MembershipStatus.Active;
    }

    /// <summary>Ends the membership. Kept for history; sets status and date atomically.</summary>
    public void Leave()
    {
        if (Status == MembershipStatus.Left)
            return;

        Status = MembershipStatus.Left;
        LeftAtUtc = DateTime.UtcNow;
    }

    private void EnsureNotLeft()
    {
        if (Status == MembershipStatus.Left)
            throw new DomainException("A membership that has left cannot be modified.");
    }
}
