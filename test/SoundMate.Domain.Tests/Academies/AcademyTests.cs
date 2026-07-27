using Shouldly;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Academies;

public class AcademyTests
{
    private static Academy AnAcademy()
        => Academy.Create("Do Re Mi", AcademyType.Academy, Slug.Create("do-re-mi"), UserId.New());

    [Fact]
    public void Create_Valid_SetsDefaults()
    {
        var owner = UserId.New();
        var academy = Academy.Create("Do Re Mi", AcademyType.SoloTeacher, Slug.Create("do-re-mi"), owner);

        academy.Name.ShouldBe("Do Re Mi");
        academy.Type.ShouldBe(AcademyType.SoloTeacher);
        academy.OwnerId.ShouldBe(owner);
        academy.Plan.ShouldBe(SubscriptionPlan.Free);
        academy.Status.ShouldBe(AcademyStatus.Active);
        academy.Id.ShouldNotBe(default);
        academy.CreatedAtUtc.ShouldNotBe(default);
    }

    [Fact]
    public void Create_TrimsName()
        => Academy.Create("  Do Re Mi  ", AcademyType.Academy, Slug.Create("x"), UserId.New())
            .Name.ShouldBe("Do Re Mi");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_EmptyName_Throws(string? name)
        => Should.Throw<DomainException>(
            () => Academy.Create(name!, AcademyType.Academy, Slug.Create("x"), UserId.New()));

    [Fact]
    public void Create_EmptyOwner_Throws()
        => Should.Throw<DomainException>(
            () => Academy.Create("Do Re Mi", AcademyType.Academy, Slug.Create("x"), default));

    [Fact]
    public void Create_UndefinedType_Throws()
        => Should.Throw<DomainException>(
            () => Academy.Create("Do Re Mi", (AcademyType)99, Slug.Create("x"), UserId.New()));

    [Fact]
    public void ChangePlan_UpdatesPlan()
    {
        var academy = AnAcademy();
        academy.ChangePlan(SubscriptionPlan.Pro);
        academy.Plan.ShouldBe(SubscriptionPlan.Pro);
    }

    [Fact]
    public void SuspendAndActivate_ChangeStatus()
    {
        var academy = AnAcademy();

        academy.Suspend();
        academy.Status.ShouldBe(AcademyStatus.Suspended);

        academy.Activate();
        academy.Status.ShouldBe(AcademyStatus.Active);
    }

    [Fact]
    public void Cancel_ThenSuspend_Throws()
    {
        var academy = AnAcademy();
        academy.Cancel();
        Should.Throw<DomainException>(() => academy.Suspend());
    }

    [Fact]
    public void Cancel_ThenActivate_Throws()
    {
        var academy = AnAcademy();
        academy.Cancel();
        Should.Throw<DomainException>(() => academy.Activate());
    }
}
