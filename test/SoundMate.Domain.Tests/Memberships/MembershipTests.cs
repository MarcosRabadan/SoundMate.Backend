using Shouldly;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Common;
using SoundMate.Domain.Memberships;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Memberships;

public class MembershipTests
{
    private static Membership AMembership()
        => Membership.Create(UserId.New(), AcademyId.New(), MembershipRole.Student);

    [Fact]
    public void Create_Valid_SetsActiveAndJoinedDate()
    {
        var membership = AMembership();

        membership.Status.ShouldBe(MembershipStatus.Active);
        membership.Role.ShouldBe(MembershipRole.Student);
        membership.JoinedAtUtc.ShouldNotBe(default);
        membership.LeftAtUtc.ShouldBeNull();
    }

    [Fact]
    public void Create_EmptyUser_Throws()
        => Should.Throw<DomainException>(
            () => Membership.Create(default, AcademyId.New(), MembershipRole.Student));

    [Fact]
    public void Create_EmptyAcademy_Throws()
        => Should.Throw<DomainException>(
            () => Membership.Create(UserId.New(), default, MembershipRole.Student));

    [Fact]
    public void Create_UndefinedRole_Throws()
        => Should.Throw<DomainException>(
            () => Membership.Create(UserId.New(), AcademyId.New(), (MembershipRole)99));

    [Fact]
    public void Leave_SetsStatusAndDateTogether()
    {
        var membership = AMembership();

        membership.Leave();

        membership.Status.ShouldBe(MembershipStatus.Left);
        membership.LeftAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public void Leave_Twice_IsIdempotent()
    {
        var membership = AMembership();
        membership.Leave();
        var firstDate = membership.LeftAtUtc;

        membership.Leave();

        membership.LeftAtUtc.ShouldBe(firstDate);
    }

    [Fact]
    public void PauseAndResume_ChangeStatus()
    {
        var membership = AMembership();

        membership.Pause();
        membership.Status.ShouldBe(MembershipStatus.Paused);

        membership.Resume();
        membership.Status.ShouldBe(MembershipStatus.Active);
    }

    [Fact]
    public void ChangeRole_UpdatesRole()
    {
        var membership = AMembership();
        membership.ChangeRole(MembershipRole.Teacher);
        membership.Role.ShouldBe(MembershipRole.Teacher);
    }

    [Fact]
    public void AfterLeaving_CannotChangeRole()
    {
        var membership = AMembership();
        membership.Leave();
        Should.Throw<DomainException>(() => membership.ChangeRole(MembershipRole.Teacher));
    }

    [Fact]
    public void AfterLeaving_CannotPause()
    {
        var membership = AMembership();
        membership.Leave();
        Should.Throw<DomainException>(() => membership.Pause());
    }
}
