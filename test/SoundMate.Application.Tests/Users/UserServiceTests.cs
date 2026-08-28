using Shouldly;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Tests.Fakes;
using SoundMate.Application.Users;
using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Common;
using SoundMate.Domain.Memberships;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Users;

public class UserServiceTests
{
    private const string Password = "Str0ngPass!";

    private readonly FakeUserRepository _users = new();
    private readonly FakeMembershipRepository _memberships = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly UserService _service;

    public UserServiceTests()
        => _service = new UserService(_users, _memberships, _unitOfWork, _hasher);

    private static RegisterUserDto Request(string email = "ana@example.com") => new()
    {
        Email = email,
        Password = Password,
        FullName = "Ana García",
        Phone = "600123123"
    };

    /// <summary>Registers through the service so the stored hash is the real one.</summary>
    private Task<UserDto> RegisterAsync(string email = "ana@example.com")
        => _service.RegisterAsync(Request(email));

    // ---------------------------------------------------------------- register

    [Fact]
    public async Task Registers_a_new_user_and_commits()
    {
        var dto = await RegisterAsync();

        dto.Email.ShouldBe("ana@example.com");
        dto.FullName.ShouldBe("Ana García");
        dto.Status.ShouldBe(UserStatus.Active);
        dto.Id.ShouldNotBe(Guid.Empty);

        _users.Added.Count.ShouldBe(1);
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Stores_the_hash_and_never_the_password()
    {
        await RegisterAsync();

        var stored = _users.Added.Single();

        stored.PasswordHash.ShouldNotBe(Password);
        stored.PasswordHash.ShouldStartWith(FakePasswordHasher.Prefix);
        _hasher.Verify(Password, stored.PasswordHash).ShouldBeTrue();
    }

    [Fact]
    public async Task Rejects_an_email_that_is_already_taken()
    {
        _users.Seed(User.Register(Email.Create("ana@example.com"), "hash", "Ana"));

        var ex = await Should.ThrowAsync<EmailAlreadyRegisteredException>(() => RegisterAsync());

        ex.Email.ShouldBe("ana@example.com");
        _users.Added.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Treats_the_email_as_case_insensitive()
    {
        // One email is one person globally. The citext column enforces it in the database; this
        // makes sure the check in front of it agrees, instead of waving through a duplicate for
        // the index to reject.
        _users.Seed(User.Register(Email.Create("ana@example.com"), "hash", "Ana"));

        await Should.ThrowAsync<EmailAlreadyRegisteredException>(
            () => RegisterAsync("ANA@EXAMPLE.COM"));
    }

    [Fact]
    public async Task Answers_409_not_500_when_it_loses_the_race_against_the_unique_index()
    {
        // Two registrations for the same email in flight: both passed the existence check, and
        // the index rejected this one. Unhandled, Postgres' 23505 reaches the caller as a 500.
        _unitOfWork.FailWithUniqueViolationOn = "IX_Users_Email";

        var ex = await Should.ThrowAsync<EmailAlreadyRegisteredException>(() => RegisterAsync());

        ex.Email.ShouldBe("ana@example.com");
        ex.InnerException.ShouldBeOfType<UniqueConstraintViolationException>();
    }

    [Fact]
    public async Task Does_not_disguise_a_violation_of_some_other_index_as_a_duplicate_email()
    {
        // Users has one unique index today. If a second one is ever added, its violations must
        // keep surfacing as themselves rather than becoming a misleading "email taken".
        _unitOfWork.FailWithUniqueViolationOn = "IX_Users_SomethingElse";

        await Should.ThrowAsync<UniqueConstraintViolationException>(() => RegisterAsync());
    }

    [Fact]
    public async Task Rejects_a_malformed_email_before_touching_the_database()
    {
        await Should.ThrowAsync<DomainException>(() => RegisterAsync("not-an-email"));

        _users.Added.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    // ---------------------------------------------------------------- reads

    [Fact]
    public async Task Returns_null_for_an_unknown_id()
        => (await _service.GetByIdAsync(Guid.NewGuid())).ShouldBeNull();

    [Fact]
    public async Task Finds_a_user_by_id()
    {
        var registered = await RegisterAsync();

        var found = await _service.GetByIdAsync(registered.Id);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(registered.Id);
    }

    [Theory]
    [InlineData("ana@example.com")]
    [InlineData("ANA@EXAMPLE.COM")]
    [InlineData("  ana@example.com  ")]
    public async Task Finds_a_user_by_email_however_it_is_typed(string lookup)
    {
        var registered = await RegisterAsync();

        var found = await _service.GetByEmailAsync(lookup);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(registered.Id);
    }

    [Fact]
    public async Task Returns_null_for_an_email_nobody_has()
        => (await _service.GetByEmailAsync("nobody@example.com")).ShouldBeNull();

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("")]
    public async Task A_malformed_email_matches_nobody_rather_than_failing(string lookup)
    {
        // Throwing here would turn "found nothing" into a 400, and would confirm to whoever is
        // probing that their input at least parsed.
        (await _service.GetByEmailAsync(lookup)).ShouldBeNull();
    }

    // ---------------------------------------------------------------- update

    [Fact]
    public async Task Updates_the_name_and_the_phone()
    {
        var registered = await RegisterAsync();

        var updated = await _service.UpdateAsync(registered.Id, new UpdateUserDto
        {
            FullName = "Ana García López",
            Phone = "699888777"
        });

        updated.FullName.ShouldBe("Ana García López");
        updated.Phone.ShouldBe("699888777");
        updated.Email.ShouldBe("ana@example.com");   // untouched: it is the person's identity
    }

    [Fact]
    public async Task Clears_the_phone_when_it_is_sent_as_null()
    {
        var registered = await RegisterAsync();

        var updated = await _service.UpdateAsync(registered.Id,
            new UpdateUserDto { FullName = "Ana García", Phone = null });

        updated.Phone.ShouldBeNull();
    }

    [Fact]
    public async Task Refuses_to_update_a_user_that_does_not_exist()
    {
        await Should.ThrowAsync<UserNotFoundException>(
            () => _service.UpdateAsync(Guid.NewGuid(), new UpdateUserDto { FullName = "Nadie" }));

        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Does_not_half_apply_an_update_whose_name_the_domain_rejects()
    {
        var registered = await RegisterAsync();
        _unitOfWork.SaveCount.ShouldBe(1);

        await Should.ThrowAsync<DomainException>(
            () => _service.UpdateAsync(registered.Id, new UpdateUserDto { FullName = "  ", Phone = "1" }));

        // Rename threw before ChangePhone ran, and nothing was committed.
        _unitOfWork.SaveCount.ShouldBe(1);
        (await _service.GetByIdAsync(registered.Id))!.Phone.ShouldBe("600123123");
    }

    // ---------------------------------------------------------------- password

    [Fact]
    public async Task Changes_the_password_when_the_current_one_matches()
    {
        var registered = await RegisterAsync();

        await _service.ChangePasswordAsync(registered.Id, new ChangePasswordDto
        {
            CurrentPassword = Password,
            NewPassword = "An0therPass!"
        });

        var stored = _users.Added.Single();
        _hasher.Verify("An0therPass!", stored.PasswordHash).ShouldBeTrue();
        _hasher.Verify(Password, stored.PasswordHash).ShouldBeFalse();
    }

    [Fact]
    public async Task Refuses_to_change_the_password_without_the_current_one()
    {
        // Without this check, reaching the endpoint is the same as owning the account.
        var registered = await RegisterAsync();
        var before = _users.Added.Single().PasswordHash;

        await Should.ThrowAsync<IncorrectPasswordException>(
            () => _service.ChangePasswordAsync(registered.Id, new ChangePasswordDto
            {
                CurrentPassword = "wrong",
                NewPassword = "An0therPass!"
            }));

        _users.Added.Single().PasswordHash.ShouldBe(before);
        _unitOfWork.SaveCount.ShouldBe(1);   // only the registration
    }

    [Fact]
    public async Task Refuses_to_change_the_password_of_a_user_that_does_not_exist()
        => await Should.ThrowAsync<UserNotFoundException>(
            () => _service.ChangePasswordAsync(Guid.NewGuid(), new ChangePasswordDto
            {
                CurrentPassword = Password,
                NewPassword = "An0therPass!"
            }));

    // ---------------------------------------------------------------- status

    [Fact]
    public async Task Suspends_and_reactivates()
    {
        var registered = await RegisterAsync();

        (await _service.SuspendAsync(registered.Id)).Status.ShouldBe(UserStatus.Suspended);
        (await _service.ReactivateAsync(registered.Id)).Status.ShouldBe(UserStatus.Active);
    }

    [Fact]
    public async Task Verifies_the_email()
    {
        var registered = await RegisterAsync();

        await _service.VerifyEmailAsync(registered.Id);

        _users.Added.Single().EmailVerifiedAtUtc.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("suspend")]
    [InlineData("reactivate")]
    [InlineData("verify")]
    public async Task Refuses_to_change_the_status_of_a_user_that_does_not_exist(string operation)
    {
        var id = Guid.NewGuid();

        Func<Task> act = operation switch
        {
            "suspend" => () => _service.SuspendAsync(id),
            "reactivate" => () => _service.ReactivateAsync(id),
            _ => () => _service.VerifyEmailAsync(id)
        };

        await Should.ThrowAsync<UserNotFoundException>(act);
    }

    // ---------------------------------------------------------------- soft delete

    [Fact]
    public async Task A_soft_deleted_user_stops_showing_up_in_reads()
    {
        var registered = await RegisterAsync();

        await _service.DeleteAsync(registered.Id);

        (await _service.GetByIdAsync(registered.Id)).ShouldBeNull();
        (await _service.GetByEmailAsync("ana@example.com")).ShouldBeNull();
    }

    [Fact]
    public async Task A_soft_deleted_user_keeps_their_row()
    {
        // The whole point: eight tables reference this id without a foreign key, so the row has
        // to survive for them to keep pointing at something real.
        var registered = await RegisterAsync();

        await _service.DeleteAsync(registered.Id);

        var stored = _users.Added.Single();
        stored.IsDeleted.ShouldBeTrue();
        stored.DeletedAtUtc.ShouldNotBeNull();
        stored.Id.Value.ShouldBe(registered.Id);
    }

    [Fact]
    public async Task Deleting_twice_is_not_an_error()
    {
        var registered = await RegisterAsync();

        await _service.DeleteAsync(registered.Id);
        await Should.NotThrowAsync(() => _service.DeleteAsync(registered.Id));
    }

    [Theory]
    [InlineData("update")]
    [InlineData("password")]
    [InlineData("suspend")]
    [InlineData("reactivate")]
    [InlineData("verify")]
    public async Task A_soft_deleted_user_cannot_be_modified(string operation)
    {
        var registered = await RegisterAsync();
        await _service.DeleteAsync(registered.Id);
        var id = registered.Id;

        Func<Task> act = operation switch
        {
            "update" => () => _service.UpdateAsync(id, new UpdateUserDto { FullName = "Otra" }),
            "password" => () => _service.ChangePasswordAsync(id, new ChangePasswordDto
            {
                CurrentPassword = Password,
                NewPassword = "An0therPass!"
            }),
            "suspend" => () => _service.SuspendAsync(id),
            "reactivate" => () => _service.ReactivateAsync(id),
            _ => () => _service.VerifyEmailAsync(id)
        };

        // "Not found", not "deleted": as far as every ordinary operation is concerned they are
        // gone, and saying otherwise would leak that the account exists.
        await Should.ThrowAsync<UserNotFoundException>(act);
    }

    [Fact]
    public async Task A_soft_deleted_user_keeps_their_email_reserved()
    {
        // Handing the address to somebody new would create a second person wearing the first
        // one's identity, while eight tables still point at the original id.
        await RegisterAsync();
        var registered = _users.Added.Single();
        await _service.DeleteAsync(registered.Id.Value);

        await Should.ThrowAsync<EmailAlreadyRegisteredException>(() => RegisterAsync());
    }

    // ---------------------------------------------------------------- restore

    [Fact]
    public async Task Restores_a_soft_deleted_user()
    {
        var registered = await RegisterAsync();
        await _service.DeleteAsync(registered.Id);

        var restored = await _service.RestoreAsync(registered.Id);

        restored.Id.ShouldBe(registered.Id);
        (await _service.GetByIdAsync(registered.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task Restoring_brings_back_the_suspension_untouched()
    {
        // This is why deletion is not a UserStatus value. Folded into the enum, deleting a
        // suspended user would forget the suspension and restoring would have to guess.
        var registered = await RegisterAsync();
        await _service.SuspendAsync(registered.Id);
        await _service.DeleteAsync(registered.Id);

        var restored = await _service.RestoreAsync(registered.Id);

        restored.Status.ShouldBe(UserStatus.Suspended);
    }

    [Fact]
    public async Task Restoring_a_user_who_was_never_deleted_just_returns_them()
    {
        var registered = await RegisterAsync();

        (await _service.RestoreAsync(registered.Id)).Id.ShouldBe(registered.Id);
    }

    [Fact]
    public async Task Refuses_to_restore_a_user_that_does_not_exist()
        => await Should.ThrowAsync<UserNotFoundException>(() => _service.RestoreAsync(Guid.NewGuid()));

    // ---------------------------------------------------------------- purge

    [Fact]
    public async Task Purges_a_user_who_belongs_nowhere()
    {
        var registered = await RegisterAsync();

        await _service.PurgeAsync(registered.Id);

        (await _service.GetByIdAsync(registered.Id)).ShouldBeNull();

        // And it is irreversible, which is the whole difference from DeleteAsync: there is no
        // row left for a restore to find.
        await Should.ThrowAsync<UserNotFoundException>(() => _service.RestoreAsync(registered.Id));
    }

    [Fact]
    public async Task Refuses_to_purge_a_user_who_still_belongs_to_an_academy()
    {
        // Nothing in the database stops this: aggregates reference each other by identity with no
        // enforced foreign keys, so the delete would just orphan the membership silently.
        var registered = await RegisterAsync();
        _memberships.Seed(Membership.Create(UserId.From(registered.Id), AcademyId.New(), MembershipRole.Teacher));

        var ex = await Should.ThrowAsync<UserStillHasMembershipsException>(
            () => _service.PurgeAsync(registered.Id));

        ex.MembershipCount.ShouldBe(1);
        (await _service.GetByIdAsync(registered.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task Purging_ignores_memberships_that_belong_to_somebody_else()
    {
        var registered = await RegisterAsync();
        _memberships.Seed(Membership.Create(UserId.New(), AcademyId.New(), MembershipRole.Student));

        await Should.NotThrowAsync(() => _service.PurgeAsync(registered.Id));
    }

    [Fact]
    public async Task Purges_a_user_who_was_already_soft_deleted()
    {
        // The normal order: soft delete first, purge later when a retention window expires.
        var registered = await RegisterAsync();
        await _service.DeleteAsync(registered.Id);

        await Should.NotThrowAsync(() => _service.PurgeAsync(registered.Id));
    }

    [Fact]
    public async Task Refuses_to_purge_a_user_that_does_not_exist()
        => await Should.ThrowAsync<UserNotFoundException>(() => _service.PurgeAsync(Guid.NewGuid()));

    [Fact]
    public async Task Soft_deleting_does_not_care_about_memberships()
    {
        // Unlike a purge: nothing is orphaned, the id survives, and it is reversible.
        var registered = await RegisterAsync();
        _memberships.Seed(Membership.Create(UserId.From(registered.Id), AcademyId.New(), MembershipRole.Teacher));

        await Should.NotThrowAsync(() => _service.DeleteAsync(registered.Id));
    }
}
