using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Users;

public class UserTests
{
    private static Email AnEmail() => Email.Create("ana@mail.com");

    private static User ARegisteredUser() => User.Register(AnEmail(), "hash", "Ana García");

    [Fact]
    public void Register_Valid_SetsFields()
    {
        var user = User.Register(AnEmail(), "hash", "  Ana García  ", "  600 123 123  ");

        user.Email.ShouldBe(AnEmail());
        user.PasswordHash.ShouldBe("hash");
        user.FullName.ShouldBe("Ana García");   // trimmed
        user.Phone.ShouldBe("600 123 123");      // trimmed
        user.Status.ShouldBe(UserStatus.Active);
        user.Id.ShouldNotBe(default);
        user.CreatedAtUtc.ShouldNotBe(default);
        user.UpdatedAtUtc.ShouldBe(user.CreatedAtUtc);
        user.EmailVerifiedAtUtc.ShouldBeNull();
    }

    [Fact]
    public void Register_WithoutPhone_LeavesItNull()
        => ARegisteredUser().Phone.ShouldBeNull();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_EmptyPasswordHash_Throws(string? hash)
        => Should.Throw<DomainException>(() => User.Register(AnEmail(), hash!, "Ana"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_EmptyFullName_Throws(string? name)
        => Should.Throw<DomainException>(() => User.Register(AnEmail(), "hash", name!));

    [Fact]
    public void Register_NullEmail_Throws()
        => Should.Throw<ArgumentNullException>(() => User.Register(null!, "hash", "Ana"));

    [Fact]
    public void Rename_Valid_UpdatesNameAndTimestamp()
    {
        var user = ARegisteredUser();
        var before = user.UpdatedAtUtc;

        user.Rename("Ana Ruiz");

        user.FullName.ShouldBe("Ana Ruiz");
        user.UpdatedAtUtc.ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void Rename_Empty_Throws()
        => Should.Throw<DomainException>(() => ARegisteredUser().Rename(" "));

    [Fact]
    public void ChangePasswordHash_Empty_Throws()
        => Should.Throw<DomainException>(() => ARegisteredUser().ChangePasswordHash(""));

    [Fact]
    public void VerifyEmail_SetsTimestamp()
    {
        var user = ARegisteredUser();
        user.VerifyEmail();
        user.EmailVerifiedAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public void SuspendAndReactivate_ChangeStatus()
    {
        var user = ARegisteredUser();

        user.Suspend();
        user.Status.ShouldBe(UserStatus.Suspended);

        user.Reactivate();
        user.Status.ShouldBe(UserStatus.Active);
    }

    [Fact]
    public void ChangePhone_Null_ClearsIt()
    {
        var user = User.Register(AnEmail(), "hash", "Ana", "600");
        user.ChangePhone(null);
        user.Phone.ShouldBeNull();
    }
}
