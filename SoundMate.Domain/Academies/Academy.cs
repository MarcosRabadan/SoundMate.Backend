using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Academies;

/// <summary>
/// The organization: an academy with several teachers, or a private teacher (an academy of
/// type <see cref="AcademyType.SoloTeacher"/>). References its owner by <see cref="UserId"/>.
/// A cancelled academy cannot be suspended or reactivated.
/// </summary>
public sealed class Academy : AggregateRoot<AcademyId>
{
    public string Name { get; private set; } = default!;
    public AcademyType Type { get; private set; }
    public Slug Slug { get; private set; } = default!;
    public UserId OwnerId { get; private set; }
    public SubscriptionPlan Plan { get; private set; }
    public AcademyStatus Status { get; private set; }
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
        Name = Guard.NotNullOrWhiteSpace(name, "Academy name");
        Touch();
    }

    public void ChangeSlug(Slug slug)
    {
        ArgumentNullException.ThrowIfNull(slug);
        Slug = slug;
        Touch();
    }

    public void ChangePlan(SubscriptionPlan plan)
    {
        Plan = Guard.Defined(plan, "Plan");
        Touch();
    }

    public void Suspend()
    {
        EnsureNotCancelled();
        Status = AcademyStatus.Suspended;
        Touch();
    }

    public void Activate()
    {
        EnsureNotCancelled();
        Status = AcademyStatus.Active;
        Touch();
    }

    public void Cancel()
    {
        Status = AcademyStatus.Cancelled;
        Touch();
    }

    private void EnsureNotCancelled()
    {
        if (Status == AcademyStatus.Cancelled)
            throw new DomainException("A cancelled academy cannot change its status.");
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
