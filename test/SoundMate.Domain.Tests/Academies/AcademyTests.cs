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

    // ------------------------------------------------------------------ guards

    public static TheoryData<string, Action<Academy>> Mutations => new()
    {
        { "Rename", a => a.Rename("Otro Nombre") },
        { "ChangeSlug", a => a.ChangeSlug(Slug.Create("otro-slug")) },
        { "ChangePlan", a => a.ChangePlan(SubscriptionPlan.Pro) },
        { "Suspend", a => a.Suspend() },
        { "Activate", a => a.Activate() }
    };

    [Theory]
    [MemberData(nameof(Mutations))]
    public void ACancelledAcademy_CannotBeModified(string name, Action<Academy> mutate)
    {
        // Rename, ChangeSlug and ChangePlan used to go through on a cancelled academy: the guard
        // only covered the two status changes, which contradicted the type's own documentation.
        var academy = AnAcademy();
        academy.Cancel();

        Should.Throw<DomainException>(() => mutate(academy),
            $"{name} should be refused on a cancelled academy.");
    }

    [Theory]
    [MemberData(nameof(Mutations))]
    public void ADeletedAcademy_CannotBeModified(string name, Action<Academy> mutate)
    {
        var academy = AnAcademy();
        academy.Delete();

        Should.Throw<DomainException>(() => mutate(academy),
            $"{name} should be refused on a deleted academy.");
    }

    [Fact]
    public void ADeletedAcademy_CannotBeCancelled()
        => Should.Throw<DomainException>(() =>
        {
            var academy = AnAcademy();
            academy.Delete();
            academy.Cancel();
        });

    // ------------------------------------------------------------------ soft delete

    [Fact]
    public void Create_IsNotDeleted()
    {
        var academy = AnAcademy();

        academy.IsDeleted.ShouldBeFalse();
        academy.DeletedAtUtc.ShouldBeNull();
    }

    [Fact]
    public void Delete_StampsTheDate()
    {
        var academy = AnAcademy();

        academy.Delete();

        academy.IsDeleted.ShouldBeTrue();
        academy.DeletedAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public void Delete_Twice_DoesNotMoveTheDate()
    {
        var academy = AnAcademy();

        academy.Delete();
        var first = academy.DeletedAtUtc;
        academy.Delete();

        academy.DeletedAtUtc.ShouldBe(first);
    }

    [Fact]
    public void Restore_OnALiveAcademy_DoesNothing()
        => Should.NotThrow(() => AnAcademy().Restore());

    [Fact]
    public void Delete_DoesNotTouchTheStatus()
    {
        // The two are independent facts: Cancelled is where the business stands, DeletedAtUtc is
        // whether the record is here.
        var academy = AnAcademy();
        academy.Suspend();

        academy.Delete();

        academy.Status.ShouldBe(AcademyStatus.Suspended);
    }

    [Fact]
    public void Restore_BringsBackTheStatus()
    {
        var academy = AnAcademy();
        academy.Cancel();
        academy.Delete();

        academy.Restore();

        academy.Status.ShouldBe(AcademyStatus.Cancelled);
        academy.IsDeleted.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(Mutations))]
    public void ARestoredAcademy_CanBeModifiedAgain(string name, Action<Academy> mutate)
    {
        var academy = AnAcademy();
        academy.Delete();
        academy.Restore();

        Should.NotThrow(() => mutate(academy), $"{name} should work again after a restore.");
    }

    // ------------------------------------------------------------------ reopen

    [Fact]
    public void Reopen_BringsACancelledAcademyBack()
    {
        var academy = AnAcademy();
        academy.Cancel();

        academy.Reopen();

        academy.Status.ShouldBe(AcademyStatus.Active);
    }

    [Theory]
    [MemberData(nameof(Mutations))]
    public void AReopenedAcademy_CanBeModifiedAgain(string name, Action<Academy> mutate)
    {
        var academy = AnAcademy();
        academy.Cancel();
        academy.Reopen();

        Should.NotThrow(() => mutate(academy), $"{name} should work again after a reopen.");
    }

    [Fact]
    public void Reopen_OnALiveAcademy_DoesNothing()
    {
        var academy = AnAcademy();

        academy.Reopen();

        academy.Status.ShouldBe(AcademyStatus.Active);
    }

    [Fact]
    public void Reopen_DoesNotLiftASuspension()
    {
        // Narrow on purpose: undoing a suspension is Activate's job, and letting a reopen do it
        // silently would wave away a moderation decision.
        var academy = AnAcademy();
        academy.Suspend();

        academy.Reopen();

        academy.Status.ShouldBe(AcademyStatus.Suspended);
    }

    [Fact]
    public void ADeletedAcademy_CannotBeReopened()
    {
        // Restore it first. Reopening something the API cannot even see would be a change nobody
        // could observe.
        var academy = AnAcademy();
        academy.Cancel();
        academy.Delete();

        Should.Throw<DomainException>(() => academy.Reopen());
    }

    [Fact]
    public void Cancelled_ThenDeleted_ThenRestored_ThenReopened_Works()
    {
        // The full way back from the corner this used to be: a cancelled academy that was also
        // soft-deleted had no route to operating again.
        var academy = AnAcademy();
        academy.Cancel();
        academy.Delete();

        academy.Restore();
        academy.Status.ShouldBe(AcademyStatus.Cancelled);

        academy.Reopen();
        academy.Status.ShouldBe(AcademyStatus.Active);

        Should.NotThrow(() => academy.Rename("De vuelta"));
    }
}
