using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Users;

/// <summary>
/// <c>User</c> to <see cref="UserDto"/>, by hand.
/// <para>
/// There is no AutoMapper here on purpose. The only patched versions of it ship under a
/// commercial licence (GHSA-rvv3-g6hj-g44x is fixed in 15.1.1 and 16.1.1; everything MIT is
/// affected), and none of what it offers was being used: SoundMate's ids are typed structs and
/// its emails are value objects, so every single member had to be spelled out anyway.
/// </para>
/// <para>
/// The hand-written version is also stricter. <see cref="UserDto"/>'s members are
/// <c>required</c>, so forgetting one here does not compile — where AutoMapper would have left it
/// at its default until <c>AssertConfigurationIsValid</c> happened to run.
/// </para>
/// </summary>
internal static class UserMapper
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id.Value,
        Email = user.Email.Value,
        FullName = user.FullName,
        Phone = user.Phone,
        Status = user.Status.ToString(),
        CreatedAtUtc = user.CreatedAtUtc
    };
}
