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

    [Fact]
    public void UpdateDescription_ExceedingMaxLength_Throws()
    {
        var profile = UserProfile.Create(UserId.New());
        var tooLong = new string('a', UserProfile.MaxDescriptionLength + 1);

        Should.Throw<DomainException>(() => profile.UpdateDescription(tooLong));
    }

    [Fact]
    public void UpdateDescription_AtMaxLength_IsAccepted()
    {
        var profile = UserProfile.Create(UserId.New());

        Should.NotThrow(() => profile.UpdateDescription(new string('a', UserProfile.MaxDescriptionLength)));
    }

    // ------------------------------------------------------------------ avatar url

    [Theory]
    [InlineData("https://cdn.example.com/avatars/ana.png")]
    [InlineData("http://cdn.example.com/ana.png")]   // http too: avatars live wherever they already lived
    [InlineData("  https://cdn.example.com/ana.png  ")]
    [InlineData(null)]                               // optional: absent is valid
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidAvatarUrl_WithAcceptableInput_IsTrue(string? url)
        => UserProfile.IsValidAvatarUrl(url).ShouldBeTrue();

    [Theory]
    [InlineData("banana")]
    [InlineData("/avatars/ana.png")]                 // relative: means nothing to whoever renders it
    [InlineData("avatars/ana.png")]
    [InlineData("ftp://cdn.example.com/ana.png")]
    [InlineData("javascript:alert(1)")]
    [InlineData("cdn.example.com/ana.png")]          // no scheme
    public void IsValidAvatarUrl_WithUnacceptableInput_IsFalse(string url)
        => UserProfile.IsValidAvatarUrl(url).ShouldBeFalse();

    [Fact]
    public void IsValidAvatarUrl_ExceedingMaxLength_IsFalse()
    {
        var tooLong = "https://cdn.example.com/" + new string('a', UserProfile.MaxAvatarUrlLength);

        UserProfile.IsValidAvatarUrl(tooLong).ShouldBeFalse();
    }

    [Theory]
    [InlineData("https://cdn.example.com/ana.png")]
    [InlineData("banana")]
    [InlineData("/avatars/ana.png")]
    [InlineData(null)]
    [InlineData("")]
    public void IsValidAvatarUrl_AgreesWithUpdateAvatar(string? url)
    {
        // The reason IsValid exists at all: the request validator asks this, the aggregate
        // enforces that, and if they drift the caller gets a thrown invariant instead of a 400.
        var profile = UserProfile.Create(UserId.New());
        var updateSucceeds = true;

        try
        {
            profile.UpdateAvatar(url);
        }
        catch (DomainException)
        {
            updateSucceeds = false;
        }

        UserProfile.IsValidAvatarUrl(url).ShouldBe(updateSucceeds);
    }

    [Fact]
    public void UpdateAvatar_Invalid_Throws()
        => Should.Throw<DomainException>(() => UserProfile.Create(UserId.New()).UpdateAvatar("banana"));

    [Fact]
    public void UpdateAvatar_Null_ClearsIt()
    {
        var profile = UserProfile.Create(UserId.New());
        profile.UpdateAvatar("https://cdn.example.com/ana.png");

        profile.UpdateAvatar(null);

        profile.AvatarUrl.ShouldBeNull();
    }

    [Fact]
    public void UpdateAvatar_Invalid_LeavesThePreviousOneAlone()
    {
        var profile = UserProfile.Create(UserId.New());
        profile.UpdateAvatar("https://cdn.example.com/ana.png");

        Should.Throw<DomainException>(() => profile.UpdateAvatar("banana"));

        profile.AvatarUrl.ShouldBe("https://cdn.example.com/ana.png");
    }
}
