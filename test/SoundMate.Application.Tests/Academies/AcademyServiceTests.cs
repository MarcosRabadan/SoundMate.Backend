using Shouldly;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Application.Academies;
using SoundMate.Application.Academies.DTO;
using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Tests.Fakes;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Common;
using SoundMate.Domain.Memberships;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Academies;

public class AcademyServiceTests
{
    private readonly FakeAcademyRepository _academies = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeMembershipRepository _memberships = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly AcademyService _service;

    private readonly User _owner;

    public AcademyServiceTests()
    {
        _owner = User.Register(Email.Create("ana@example.com"), "hash", "Ana García");
        _users.Seed(_owner);

        _service = new AcademyService(_academies, _users, _memberships, _unitOfWork);
    }

    private CreateAcademyDto Request(string slug = "do-re-mi", Guid? ownerId = null) => new()
    {
        Name = "Do Re Mi",
        Type = AcademyType.Academy,
        Slug = slug,
        OwnerUserId = ownerId ?? _owner.Id.Value
    };

    private Task<AcademyDto> CreateAsync(string slug = "do-re-mi") => _service.CreateAsync(Request(slug));

    // ---------------------------------------------------------------- create

    [Fact]
    public async Task Creates_an_academy_on_the_free_plan()
    {
        var dto = await CreateAsync();

        dto.Name.ShouldBe("Do Re Mi");
        dto.Slug.ShouldBe("do-re-mi");
        dto.Type.ShouldBe(AcademyType.Academy);
        dto.Plan.ShouldBe(SubscriptionPlan.Free);
        dto.Status.ShouldBe(AcademyStatus.Active);
        dto.OwnerUserId.ShouldBe(_owner.Id.Value);
        dto.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Gives_the_owner_their_membership_in_the_same_commit()
    {
        // The anchor. Without it the academy claims an owner who, as far as
        // HasActiveMembershipAsync is concerned, does not belong to it - and that is the gate
        // every booking passes through.
        var dto = await CreateAsync();

        var membership = _memberships.Added.ShouldHaveSingleItem();
        membership.UserId.ShouldBe(_owner.Id);
        membership.AcademyId.Value.ShouldBe(dto.Id);
        membership.Role.ShouldBe(MembershipRole.Owner);

        // One SaveChanges for both rows: either they both land or neither does.
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Normalises_the_slug_before_storing_it()
    {
        var dto = await _service.CreateAsync(Request(slug: "  Do-Re-MI  "));

        dto.Slug.ShouldBe("do-re-mi");
    }

    [Fact]
    public async Task Refuses_a_slug_that_is_already_taken()
    {
        await CreateAsync();

        var ex = await Should.ThrowAsync<SlugAlreadyTakenException>(() => CreateAsync());

        ex.Slug.ShouldBe("do-re-mi");
        _academies.Added.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Compares_the_slug_after_normalising_it()
    {
        await CreateAsync();

        await Should.ThrowAsync<SlugAlreadyTakenException>(
            () => _service.CreateAsync(Request(slug: "DO-RE-MI")));
    }

    [Fact]
    public async Task Answers_409_not_500_when_it_loses_the_race_for_a_slug()
    {
        // Both creations passed the existence check and the unique index rejected this one.
        // Unhandled, Postgres' 23505 reaches the caller as a 500.
        _unitOfWork.FailWithUniqueViolationOn = "IX_Academies_Slug";

        var ex = await Should.ThrowAsync<SlugAlreadyTakenException>(() => CreateAsync());

        ex.InnerException.ShouldBeOfType<UniqueConstraintViolationException>();
    }

    [Fact]
    public async Task Does_not_disguise_a_violation_of_some_other_index_as_a_duplicate_slug()
    {
        _unitOfWork.FailWithUniqueViolationOn = "IX_Academies_SomethingElse";

        await Should.ThrowAsync<UniqueConstraintViolationException>(() => CreateAsync());
    }

    [Fact]
    public async Task Refuses_an_owner_that_does_not_exist()
    {
        await Should.ThrowAsync<UserNotFoundException>(
            () => _service.CreateAsync(Request(ownerId: Guid.NewGuid())));

        _academies.Added.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Refuses_an_owner_whose_account_is_deleted()
    {
        // An academy owned by a closed account is exactly the orphan the soft delete exists to
        // avoid creating.
        _owner.Delete();

        await Should.ThrowAsync<UserNotFoundException>(() => CreateAsync());

        _academies.Added.ShouldBeEmpty();
    }

    [Fact]
    public async Task Refuses_a_malformed_slug_before_touching_anything()
    {
        await Should.ThrowAsync<DomainException>(() => _service.CreateAsync(Request(slug: "Mi Academia!")));

        _academies.Added.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    // ---------------------------------------------------------------- reads

    [Fact]
    public async Task Returns_null_for_an_unknown_id()
        => (await _service.GetByIdAsync(Guid.NewGuid())).ShouldBeNull();

    [Theory]
    [InlineData("do-re-mi")]
    [InlineData("DO-RE-MI")]
    [InlineData("  do-re-mi  ")]
    public async Task Finds_an_academy_by_slug_however_it_is_typed(string lookup)
    {
        var created = await CreateAsync();

        var found = await _service.GetBySlugAsync(lookup);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(created.Id);
    }

    [Theory]
    [InlineData("Mi Academia!")]
    [InlineData("a--b")]
    [InlineData("")]
    public async Task A_malformed_slug_matches_nobody_rather_than_failing(string lookup)
        => (await _service.GetBySlugAsync(lookup)).ShouldBeNull();

    [Fact]
    public async Task Lists_the_academies_of_an_owner()
    {
        await CreateAsync("do-re-mi");
        await CreateAsync("fa-sol-la");

        var list = await _service.ListByOwnerAsync(_owner.Id.Value);

        list.Count.ShouldBe(2);
        list.Select(a => a.Slug).OrderBy(s => s).ShouldBe(["do-re-mi", "fa-sol-la"]);
    }

    [Fact]
    public async Task Leaves_deleted_academies_out_of_the_owner_listing()
    {
        var first = await CreateAsync("do-re-mi");
        await CreateAsync("fa-sol-la");
        await _service.DeleteAsync(first.Id);

        (await _service.ListByOwnerAsync(_owner.Id.Value)).ShouldHaveSingleItem()
            .Slug.ShouldBe("fa-sol-la");
    }

    [Fact]
    public async Task An_owner_with_nothing_gets_an_empty_list_not_a_404()
        => (await _service.ListByOwnerAsync(Guid.NewGuid())).ShouldBeEmpty();

    // ---------------------------------------------------------------- update

    [Fact]
    public async Task Renames_the_academy()
    {
        var created = await CreateAsync();

        var updated = await _service.UpdateAsync(created.Id, new UpdateAcademyDto { Name = "Do Re Mi Norte" });

        updated.Name.ShouldBe("Do Re Mi Norte");
        updated.Slug.ShouldBe("do-re-mi");   // untouched
    }

    [Fact]
    public async Task Refuses_to_update_an_academy_that_does_not_exist()
        => await Should.ThrowAsync<AcademyNotFoundException>(
            () => _service.UpdateAsync(Guid.NewGuid(), new UpdateAcademyDto { Name = "X" }));

    [Fact]
    public async Task Changes_the_slug()
    {
        var created = await CreateAsync();

        var updated = await _service.ChangeSlugAsync(created.Id, new ChangeSlugDto { Slug = "do-re-mi-sur" });

        updated.Slug.ShouldBe("do-re-mi-sur");
    }

    [Fact]
    public async Task Re_sending_the_same_slug_is_a_no_op_not_a_conflict_with_itself()
    {
        var created = await CreateAsync();

        var updated = await _service.ChangeSlugAsync(created.Id, new ChangeSlugDto { Slug = "do-re-mi" });

        updated.Slug.ShouldBe("do-re-mi");
    }

    [Fact]
    public async Task Refuses_a_slug_another_academy_already_answers_to()
    {
        var first = await CreateAsync("do-re-mi");
        await CreateAsync("fa-sol-la");

        await Should.ThrowAsync<SlugAlreadyTakenException>(
            () => _service.ChangeSlugAsync(first.Id, new ChangeSlugDto { Slug = "fa-sol-la" }));
    }

    [Fact]
    public async Task Changes_the_plan()
    {
        var created = await CreateAsync();

        var updated = await _service.ChangePlanAsync(created.Id, new ChangePlanDto { Plan = SubscriptionPlan.Pro });

        updated.Plan.ShouldBe(SubscriptionPlan.Pro);
    }

    // ---------------------------------------------------------------- status

    [Fact]
    public async Task Suspends_and_activates()
    {
        var created = await CreateAsync();

        (await _service.SuspendAsync(created.Id)).Status.ShouldBe(AcademyStatus.Suspended);
        (await _service.ActivateAsync(created.Id)).Status.ShouldBe(AcademyStatus.Active);
    }

    [Fact]
    public async Task Cancels()
        => (await _service.CancelAsync((await CreateAsync()).Id)).Status.ShouldBe(AcademyStatus.Cancelled);

    [Fact]
    public async Task Reopens_a_cancelled_academy()
    {
        var created = await CreateAsync();
        await _service.CancelAsync(created.Id);

        (await _service.ReopenAsync(created.Id)).Status.ShouldBe(AcademyStatus.Active);

        // And it accepts changes again.
        await Should.NotThrowAsync(
            () => _service.UpdateAsync(created.Id, new UpdateAcademyDto { Name = "De vuelta" }));
    }

    [Fact]
    public async Task Reopening_does_not_lift_a_suspension()
    {
        var created = await CreateAsync();
        await _service.SuspendAsync(created.Id);

        (await _service.ReopenAsync(created.Id)).Status.ShouldBe(AcademyStatus.Suspended);
    }

    [Fact]
    public async Task Reopening_an_academy_that_was_never_cancelled_just_returns_it()
    {
        var created = await CreateAsync();

        (await _service.ReopenAsync(created.Id)).Status.ShouldBe(AcademyStatus.Active);
    }

    [Fact]
    public async Task A_cancelled_and_deleted_academy_can_be_restored_then_reopened()
    {
        // The full way back. Before Reopen existed this was a dead end: restore returned the
        // academy still cancelled, and nothing could move it out of that state.
        var created = await CreateAsync();
        await _service.CancelAsync(created.Id);
        await _service.DeleteAsync(created.Id);

        (await _service.RestoreAsync(created.Id)).Status.ShouldBe(AcademyStatus.Cancelled);
        (await _service.ReopenAsync(created.Id)).Status.ShouldBe(AcademyStatus.Active);

        (await _service.GetByIdAsync(created.Id)).ShouldNotBeNull()
            .Status.ShouldBe(AcademyStatus.Active);
    }

    [Fact]
    public async Task Reopening_a_deleted_academy_says_so_instead_of_just_not_found()
    {
        // Reopen and restore undo different things and are easy to confuse. A bare 404 leaves the
        // caller holding a valid id with no way to learn which one they wanted.
        var created = await CreateAsync();
        await _service.CancelAsync(created.Id);
        await _service.DeleteAsync(created.Id);

        var ex = await Should.ThrowAsync<AcademyIsDeletedException>(() => _service.ReopenAsync(created.Id));

        ex.Message.ShouldContain("Restore it first");
    }

    [Fact]
    public async Task Reopening_a_deleted_academy_that_was_never_cancelled_says_the_same()
    {
        // The case that actually came up: deleted, never cancelled, and "reopen" was reached for.
        var created = await CreateAsync();
        await _service.DeleteAsync(created.Id);

        await Should.ThrowAsync<AcademyIsDeletedException>(() => _service.ReopenAsync(created.Id));
    }

    [Fact]
    public async Task Refuses_to_reopen_an_academy_that_does_not_exist()
        => await Should.ThrowAsync<AcademyNotFoundException>(() => _service.ReopenAsync(Guid.NewGuid()));

    [Theory]
    [InlineData("rename")]
    [InlineData("slug")]
    [InlineData("plan")]
    [InlineData("suspend")]
    [InlineData("activate")]
    public async Task A_cancelled_academy_refuses_every_change(string operation)
    {
        var created = await CreateAsync();
        await _service.CancelAsync(created.Id);
        var id = created.Id;

        Func<Task> act = operation switch
        {
            "rename" => () => _service.UpdateAsync(id, new UpdateAcademyDto { Name = "Otra" }),
            "slug" => () => _service.ChangeSlugAsync(id, new ChangeSlugDto { Slug = "otro-slug" }),
            "plan" => () => _service.ChangePlanAsync(id, new ChangePlanDto { Plan = SubscriptionPlan.Pro }),
            "suspend" => () => _service.SuspendAsync(id),
            _ => () => _service.ActivateAsync(id)
        };

        await Should.ThrowAsync<DomainException>(act);
    }

    // ---------------------------------------------------------------- soft delete

    [Fact]
    public async Task A_soft_deleted_academy_stops_showing_up_in_reads()
    {
        var created = await CreateAsync();

        await _service.DeleteAsync(created.Id);

        (await _service.GetByIdAsync(created.Id)).ShouldBeNull();
        (await _service.GetBySlugAsync("do-re-mi")).ShouldBeNull();
    }

    [Fact]
    public async Task Deleting_twice_is_not_an_error()
    {
        var created = await CreateAsync();

        await _service.DeleteAsync(created.Id);
        await Should.NotThrowAsync(() => _service.DeleteAsync(created.Id));
    }

    [Theory]
    [InlineData("rename")]
    [InlineData("plan")]
    [InlineData("suspend")]
    [InlineData("cancel")]
    public async Task A_soft_deleted_academy_cannot_be_modified(string operation)
    {
        var created = await CreateAsync();
        await _service.DeleteAsync(created.Id);
        var id = created.Id;

        Func<Task> act = operation switch
        {
            "rename" => () => _service.UpdateAsync(id, new UpdateAcademyDto { Name = "Otra" }),
            "plan" => () => _service.ChangePlanAsync(id, new ChangePlanDto { Plan = SubscriptionPlan.Pro }),
            "suspend" => () => _service.SuspendAsync(id),
            _ => () => _service.CancelAsync(id)
        };

        // "Not found", not "deleted": as far as every ordinary operation goes, it is gone.
        await Should.ThrowAsync<AcademyNotFoundException>(act);
    }

    [Fact]
    public async Task A_soft_deleted_academy_keeps_its_slug_reserved()
    {
        // Releasing it would let a stale public link resolve to a different academy - worse than
        // the link simply breaking.
        var created = await CreateAsync();
        await _service.DeleteAsync(created.Id);

        await Should.ThrowAsync<SlugAlreadyTakenException>(() => CreateAsync());
    }

    // ---------------------------------------------------------------- restore

    [Fact]
    public async Task Restores_a_soft_deleted_academy()
    {
        var created = await CreateAsync();
        await _service.DeleteAsync(created.Id);

        var restored = await _service.RestoreAsync(created.Id);

        restored.Id.ShouldBe(created.Id);
        (await _service.GetByIdAsync(created.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task Restoring_brings_back_the_cancellation_untouched()
    {
        // Why deletion is not a fourth AcademyStatus: folded into the enum, cancelling and
        // deleting would overwrite each other and this could not work.
        var created = await CreateAsync();
        await _service.CancelAsync(created.Id);
        await _service.DeleteAsync(created.Id);

        (await _service.RestoreAsync(created.Id)).Status.ShouldBe(AcademyStatus.Cancelled);
    }

    [Fact]
    public async Task Restoring_an_academy_that_was_never_deleted_just_returns_it()
    {
        var created = await CreateAsync();

        (await _service.RestoreAsync(created.Id)).Id.ShouldBe(created.Id);
    }

    // ---------------------------------------------------------------- purge

    [Fact]
    public async Task Refuses_to_purge_an_academy_that_still_has_members()
    {
        // Creating one always leaves the owner's membership behind, so this is the default case,
        // not an edge one.
        var created = await CreateAsync();

        var ex = await Should.ThrowAsync<AcademyStillHasMembersException>(
            () => _service.PurgeAsync(created.Id));

        ex.MemberCount.ShouldBe(1);
        (await _service.GetByIdAsync(created.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task Purges_an_academy_nobody_belongs_to()
    {
        var created = await CreateAsync();
        foreach (var membership in _memberships.Added.ToList())
        {
            membership.Leave();
            _memberships.Remove(membership);
        }

        await _service.PurgeAsync(created.Id);

        (await _service.GetByIdAsync(created.Id)).ShouldBeNull();
        await Should.ThrowAsync<AcademyNotFoundException>(() => _service.RestoreAsync(created.Id));
    }

    [Fact]
    public async Task Purging_ignores_members_of_another_academy()
    {
        var created = await CreateAsync("do-re-mi");
        foreach (var membership in _memberships.Added.ToList())
            _memberships.Remove(membership);

        _memberships.Seed(Membership.Create(UserId.New(), AcademyId.New(), MembershipRole.Student));

        await Should.NotThrowAsync(() => _service.PurgeAsync(created.Id));
    }

    [Fact]
    public async Task Refuses_to_purge_an_academy_that_does_not_exist()
        => await Should.ThrowAsync<AcademyNotFoundException>(() => _service.PurgeAsync(Guid.NewGuid()));

    [Fact]
    public async Task Soft_deleting_does_not_care_about_members()
    {
        // Unlike a purge: nothing is orphaned, the id survives, and it is reversible.
        var created = await CreateAsync();

        await Should.NotThrowAsync(() => _service.DeleteAsync(created.Id));
    }
}
