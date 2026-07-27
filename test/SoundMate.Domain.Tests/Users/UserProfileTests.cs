using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Users;

public class UserProfileTests
{
    [Fact]
    public void Create_Valid_SetsUserAndTimestamp()
    {
        var userId = UserId.New();
        var profile = UserProfile.Create(userId);

        profile.UserId.ShouldBe(userId);
        profile.UpdatedAtUtc.ShouldNotBe(default);
        profile.Description.ShouldBeNull();
        profile.AvatarUrl.ShouldBeNull();
    }

    [Fact]
    public void Create_EmptyUser_Throws()
        => Should.Throw<DomainException>(() => UserProfile.Create(default));

    [Fact]
    public void UpdateDescription_Trims()
    {
        var profile = UserProfile.Create(UserId.New());
        profile.UpdateDescription("  Piano teacher  ");
        profile.Description.ShouldBe("Piano teacher");
    }

    [Fact]
    public void UpdateDescription_Whitespace_ClearsIt()
    {
        var profile = UserProfile.Create(UserId.New());
        profile.UpdateDescription("something");
        profile.UpdateDescription("   ");
        profile.Description.ShouldBeNull();
    }

    [Fact]
    public void UpdateAvatar_SetsUrl()
    {
        var profile = UserProfile.Create(UserId.New());
        profile.UpdateAvatar("https://cdn/x.png");
        profile.AvatarUrl.ShouldBe("https://cdn/x.png");
    }
}
