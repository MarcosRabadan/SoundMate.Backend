using SoundMate.Domain.Common;

namespace SoundMate.Domain.Users;

/// <summary>
/// A user's public, LinkedIn-style profile: bio and avatar. One per user (1:1), for anyone
/// (a student can have a description and education too, not only teachers).
/// </summary>
public sealed class UserProfile : AggregateRoot<UserProfileId>
{
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
        Description = Normalize(description);
        Touch();
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = Normalize(avatarUrl);
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

    private static string? Normalize(string? text)
        => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
