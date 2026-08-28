using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Academies;

/// <summary>
/// The organization: an academy with several teachers, or a private teacher (an academy of
/// type <see cref="AcademyType.SoloTeacher"/>). References its owner by <see cref="UserId"/>.
/// <para>
/// It has two independent states. <see cref="Status"/> is where the business stands — running,
/// suspended, or closed — and <see cref="DeletedAtUtc"/> is whether the record is still here.
/// Being cancelled or deleted refuses every other change, and each has exactly one way out:
/// <see cref="Reopen"/> and <see cref="Restore"/> respectively. Neither is a dead end, and
/// neither overwrites the other — a cancelled academy that is deleted comes back cancelled.
/// </para>
/// </summary>
public sealed class Academy : AggregateRoot<AcademyId>
{
    public string Name { get; private set; } = default!;
    public AcademyType Type { get; private set; }
    public Slug Slug { get; private set; } = default!;
    public UserId OwnerId { get; private set; }
    public SubscriptionPlan Plan { get; private set; }
    public AcademyStatus Status { get; private set; }

    /// <summary>
    /// When the academy was soft-deleted, or <c>null</c> while it is still here.
    /// <para>
    /// Independent of <see cref="Status"/>, and deliberately not a fourth
    /// <see cref="AcademyStatus"/> value. <c>Cancelled</c> is a <b>business</b> state — the
    /// subscription ended, the school closed — and a cancelled academy stays queryable for
    /// history, billing and the reviews written while it ran. Deletion is a fact about the
    /// <b>record</b>. Folded into one enum, cancelling and deleting would overwrite each other and
    /// a restore would have no state to return to.
    /// </para>
    /// </summary>
    public DateTime? DeletedAtUtc { get; private set; }

    /// <summary>True once <see cref="Delete"/> ran and <see cref="Restore"/> has not.</summary>
    public bool IsDeleted => DeletedAtUtc is not null;

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Academy() { }

    private Academy(AcademyId id, string name, AcademyType type, Slug slug, UserId ownerId) : base(id)
    {
        Name = name;
        Type = type;
        Slug = slug;
        OwnerId = ownerId;
        Plan = SubscriptionPlan.Free;
        Status = AcademyStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static Academy Create(string name, AcademyType type, Slug slug, UserId ownerId)
    {
        ArgumentNullException.ThrowIfNull(slug);

        return new Academy(
            AcademyId.New(),
            Guard.NotNullOrWhiteSpace(name, "Academy name"),
            Guard.Defined(type, "Academy type"),
            slug,
            Guard.NotEmpty(ownerId, "Owner"));
    }

    public void Rename(string name)
    {
        EnsureModifiable();
        Name = Guard.NotNullOrWhiteSpace(name, "Academy name");
        Touch();
    }

    public void ChangeSlug(Slug slug)
    {
        EnsureModifiable();
        ArgumentNullException.ThrowIfNull(slug);
        Slug = slug;
        Touch();
    }

    public void ChangePlan(SubscriptionPlan plan)
    {
        EnsureModifiable();
        Plan = Guard.Defined(plan, "Plan");
        Touch();
    }

    public void Suspend()
    {
        EnsureModifiable();
        Status = AcademyStatus.Suspended;
        Touch();
    }

    public void Activate()
    {
        EnsureModifiable();
        Status = AcademyStatus.Active;
        Touch();
    }

    /// <summary>
    /// Closes the academy for business: the subscription ended, or it shut down. Everything else
    /// is refused while it is cancelled — it cannot be renamed, re-slugged, re-planned, suspended
    /// or activated — but it stays readable, because its history, its billing and the reviews
    /// written while it ran do not stop existing.
    /// <para>Undone with <see cref="Reopen"/>, which is the only way out of this state.</para>
    /// </summary>
    public void Cancel()
    {
        EnsureNotDeleted();
        Status = AcademyStatus.Cancelled;
        Touch();
    }

    /// <summary>
    /// Brings a cancelled academy back into business.
    /// <para>
    /// It exists because a lapsed subscription is not a death sentence: customers come back and
    /// pay again, and that is ordinary. Without it a returning owner could never use their own
    /// academy — or its slug, which the cancelled row keeps holding — and their history and
    /// reviews would be stranded behind a door with no handle.
    /// </para>
    /// <para>
    /// Deliberately narrow: it only undoes a cancellation. A <b>suspended</b> academy stays
    /// suspended, because lifting a suspension is <see cref="Activate"/>'s job and quietly doing
    /// both here would let a reopen wave away a moderation decision.
    /// </para>
    /// </summary>
    public void Reopen()
    {
        // Not EnsureModifiable: being cancelled is the whole precondition. But a deleted academy
        // is still off limits — restore it first, then reopen it.
        EnsureNotDeleted();

        if (Status != AcademyStatus.Cancelled)
            return;

        Status = AcademyStatus.Active;
        Touch();
    }

    /// <summary>
    /// Soft-deletes the academy. The row survives so that the memberships and reviews carrying
    /// this <c>AcademyId</c> — neither of which has an enforced foreign key — keep pointing at
    /// something real. Idempotent.
    /// </summary>
    public void Delete()
    {
        if (IsDeleted)
            return;

        DeletedAtUtc = DateTime.UtcNow;
        Touch();
    }

    /// <summary>
    /// Brings a soft-deleted academy back exactly as it was, cancellation included: deleting never
    /// touched <see cref="Status"/>. Idempotent.
    /// </summary>
    public void Restore()
    {
        if (!IsDeleted)
            return;

        DeletedAtUtc = null;
        Touch();
    }

    /// <summary>
    /// The two reasons an academy stops accepting changes, in the order a caller cares about.
    /// Deleted first: a deleted academy is gone, and saying "it is cancelled" about it would be
    /// answering a question nobody asked.
    /// </summary>
    private void EnsureModifiable()
    {
        EnsureNotDeleted();
        EnsureNotCancelled();
    }

    private void EnsureNotCancelled()
    {
        if (Status == AcademyStatus.Cancelled)
            throw new DomainException("A cancelled academy cannot be modified.");
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new DomainException("A deleted academy cannot be modified. Restore it first.");
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
