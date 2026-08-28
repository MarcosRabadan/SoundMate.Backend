using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Users;

/// <summary>
/// <c>UserProfile</c> to <see cref="UserProfileDto"/>, by hand — same reasoning as
/// <c>UserMapper</c> and <c>AcademyMapper</c>.
/// </summary>
internal static class UserProfileMapper
{
    public static UserProfileDto ToDto(this UserProfile profile) => new()
    {
        UserId = profile.UserId.Value,
        Description = profile.Description,
        AvatarUrl = profile.AvatarUrl,
        UpdatedAtUtc = profile.UpdatedAtUtc
    };
}
