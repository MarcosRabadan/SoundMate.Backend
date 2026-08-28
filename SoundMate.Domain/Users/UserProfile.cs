using SoundMate.Domain.Common;

namespace SoundMate.Domain.Users;

/// <summary>
/// A user's public, LinkedIn-style profile: bio and avatar. One per user (1:1), for anyone
/// (a student can have a description and education too, not only teachers).
/// <para>
/// Both fields are optional and both can be cleared, so an empty profile is a legitimate state —
/// it means "this person has one and has not filled it in", which is different from not having one.
/// </para>
/// </summary>
public sealed class UserProfile : AggregateRoot<UserProfileId>
{
    /// <summary>Long enough for a real bio, short enough not to be a blog. Mirrored by the column.</summary>
    public const int MaxDescriptionLength = 2000;

    /// <summary>Mirrored by the column. URLs longer than this are almost always a mistake.</summary>
    public const int MaxAvatarUrlLength = 500;

    public UserId UserId { get; private set; }
    public string? Description { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private UserProfile() { }

    private UserProfile(UserProfileId id, UserId userId) : base(id)
    {
        UserId = userId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static UserProfile Create(UserId userId)
        => new(UserProfileId.New(), Guard.NotEmpty(userId, "User"));

    public void UpdateDescription(string? description)
    {
        var value = Normalize(description);

        if (value is not null && value.Length > MaxDescriptionLength)
            throw new DomainException($"Description cannot exceed {MaxDescriptionLength} characters.");

        Description = value;
        Touch();
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        if (!IsValidAvatarUrl(avatarUrl))
            throw new DomainException($"Avatar '{avatarUrl}' is not a valid absolute http or https URL.");

        AvatarUrl = Normalize(avatarUrl);
        Touch();
    }

    /// <summary>
    /// True when <see cref="UpdateAvatar"/> would accept <paramref name="avatarUrl"/>.
    /// <para>
    /// Absent counts as valid: the avatar is optional, and <c>null</c> is how you clear it.
    /// </para>
    /// <para>
    /// It exists for the same reason <c>Email.IsValid</c> does — so a request validator can answer
    /// with a per-field message using <b>this</b> rule instead of writing its own. A field called
    /// <c>AvatarUrl</c> holding "banana" is a broken invariant, not merely unhelpful data, which is
    /// why the check lives here rather than only at the edge.
    /// </para>
    /// </summary>
    public static bool IsValidAvatarUrl(string? avatarUrl)
    {
        var value = Normalize(avatarUrl);

        if (value is null)
            return true;

        if (value.Length > MaxAvatarUrlLength)
            return false;

        // Absolute only: a relative path means nothing to whoever renders this profile, since they
        // have no idea what it would be relative to. http as well as https, because avatars are
        // often served from wherever the user already had one.
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

    private static string? Normalize(string? text)
        => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
