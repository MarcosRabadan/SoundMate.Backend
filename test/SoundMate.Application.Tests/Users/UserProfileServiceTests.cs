using Shouldly;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Tests.Fakes;
using SoundMate.Application.Users;
using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Users;

public class UserProfileServiceTests
{
    private const string Avatar = "https://cdn.example.com/ana.png";

    private readonly FakeUserProfileRepository _profiles = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly UserProfileService _service;

    private readonly User _user;

    public UserProfileServiceTests()
    {
        _user = User.Register(Email.Create("ana@example.com"), "hash", "Ana García");
        _users.Seed(_user);

        _service = new UserProfileService(_profiles, _users, _unitOfWork);
    }

    private Guid UserId => _user.Id.Value;

    private static UpdateUserProfileDto Content(string? description = "Profesora de piano",
                                                string? avatar = Avatar)
        => new() { Description = description, AvatarUrl = avatar };

    // ---------------------------------------------------------------- read

    [Fact]
    public async Task Returns_null_when_the_user_has_no_profile_yet()
        => (await _service.GetByUserAsync(UserId)).ShouldBeNull();

    [Fact]
    public async Task Finds_the_profile_of_a_user_who_has_one()
    {
        await _service.SaveAsync(UserId, Content());

        var found = await _service.GetByUserAsync(UserId);

        found.ShouldNotBeNull();
        found.UserId.ShouldBe(UserId);
        found.Description.ShouldBe("Profesora de piano");
        found.AvatarUrl.ShouldBe(Avatar);
    }

    [Theory]
    [InlineData("get")]
    [InlineData("save")]
    [InlineData("delete")]
    public async Task Refuses_a_user_that_does_not_exist(string operation)
    {
        var unknown = Guid.NewGuid();

        Func<Task> act = operation switch
        {
            "get" => () => _service.GetByUserAsync(unknown),
            "save" => () => _service.SaveAsync(unknown, Content()),
            _ => () => _service.DeleteAsync(unknown)
        };

        await Should.ThrowAsync<UserNotFoundException>(act);
    }

    [Theory]
    [InlineData("get")]
    [InlineData("save")]
    [InlineData("delete")]
    public async Task Refuses_a_user_whose_account_is_deleted(string operation)
    {
        // A closed account must not keep serving — or growing — a public profile.
        await _service.SaveAsync(UserId, Content());
        _user.Delete();

        Func<Task> act = operation switch
        {
            "get" => () => _service.GetByUserAsync(UserId),
            "save" => () => _service.SaveAsync(UserId, Content()),
            _ => () => _service.DeleteAsync(UserId)
        };

        await Should.ThrowAsync<UserNotFoundException>(act);
    }

    // ---------------------------------------------------------------- save (upsert)

    [Fact]
    public async Task Creates_the_profile_when_the_user_has_none()
    {
        var dto = await _service.SaveAsync(UserId, Content());

        dto.UserId.ShouldBe(UserId);
        dto.Description.ShouldBe("Profesora de piano");
        _profiles.Added.ShouldHaveSingleItem();
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Updates_the_profile_when_the_user_already_has_one()
    {
        await _service.SaveAsync(UserId, Content());

        var dto = await _service.SaveAsync(UserId, Content(description: "Ahora enseño guitarra"));

        dto.Description.ShouldBe("Ahora enseño guitarra");

        // Updated, not duplicated: one profile per user.
        _profiles.Added.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Is_a_full_replacement_so_an_absent_field_is_cleared()
    {
        // PUT semantics: what the body does not mention is not "leave it alone", it is absent.
        await _service.SaveAsync(UserId, Content());

        var dto = await _service.SaveAsync(UserId, new UpdateUserProfileDto { Description = "Solo bio" });

        dto.Description.ShouldBe("Solo bio");
        dto.AvatarUrl.ShouldBeNull();
    }

    [Fact]
    public async Task An_empty_profile_is_a_legitimate_state()
    {
        var dto = await _service.SaveAsync(UserId, new UpdateUserProfileDto());

        dto.Description.ShouldBeNull();
        dto.AvatarUrl.ShouldBeNull();

        // And it exists: "has a profile with nothing in it" is not "has no profile".
        (await _service.GetByUserAsync(UserId)).ShouldNotBeNull();
    }

    [Fact]
    public async Task Saving_the_same_content_twice_changes_nothing()
    {
        var first = await _service.SaveAsync(UserId, Content());
        var second = await _service.SaveAsync(UserId, Content());

        second.Description.ShouldBe(first.Description);
        second.AvatarUrl.ShouldBe(first.AvatarUrl);
        second.UserId.ShouldBe(first.UserId);
    }

    [Fact]
    public async Task Rejects_an_avatar_that_is_not_an_absolute_http_url()
    {
        await Should.ThrowAsync<DomainException>(
            () => _service.SaveAsync(UserId, Content(avatar: "banana")));

        _profiles.Added.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task A_bad_avatar_does_not_half_apply_the_change()
    {
        await _service.SaveAsync(UserId, Content());

        await Should.ThrowAsync<DomainException>(
            () => _service.SaveAsync(UserId, Content(description: "Nueva bio", avatar: "banana")));

        var unchanged = await _service.GetByUserAsync(UserId);
        unchanged!.Description.ShouldBe("Profesora de piano");
        unchanged.AvatarUrl.ShouldBe(Avatar);
    }

    // ---------------------------------------------------------------- the race

    [Fact]
    public async Task Retries_instead_of_conflicting_when_it_loses_the_race_to_create()
    {
        // Two PUTs for the same brand-new profile at once: both read nothing, both insert, and the
        // unique index rejects one. A 409 would be wrong — PUT promises idempotence, and the
        // resource the caller asked for now exists.
        _unitOfWork.FailNextSaveWithUniqueViolationOn = "IX_UserProfiles_UserId";

        var dto = await _service.SaveAsync(UserId, Content());

        dto.Description.ShouldBe("Profesora de piano");
        _unitOfWork.SaveCount.ShouldBe(2);   // the insert that lost, then the update that won
    }

    [Fact]
    public async Task Does_not_disguise_a_violation_of_some_other_index()
    {
        _unitOfWork.FailNextSaveWithUniqueViolationOn = "IX_UserProfiles_SomethingElse";

        await Should.ThrowAsync<UniqueConstraintViolationException>(
            () => _service.SaveAsync(UserId, Content()));
    }

    [Fact]
    public async Task Gives_up_when_the_index_claims_a_row_that_is_not_there()
    {
        // The index said the profile exists and the re-read found nothing. That is not the race
        // this recovery was written for, so the original error has to keep travelling instead of
        // being swallowed.
        _profiles.DiscardAdds = true;
        _unitOfWork.FailNextSaveWithUniqueViolationOn = "IX_UserProfiles_UserId";

        await Should.ThrowAsync<UniqueConstraintViolationException>(
            () => _service.SaveAsync(UserId, Content()));
    }

    // ---------------------------------------------------------------- delete

    [Fact]
    public async Task Deletes_the_profile_and_leaves_the_user_alone()
    {
        await _service.SaveAsync(UserId, Content());

        await _service.DeleteAsync(UserId);

        (await _service.GetByUserAsync(UserId)).ShouldBeNull();

        // The person is still here; they just stopped having a bio.
        _users.Added.ShouldBeEmpty();
        (await _users.GetByIdAsync(_user.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task Refuses_to_delete_a_profile_that_does_not_exist()
    {
        var ex = await Should.ThrowAsync<UserProfileNotFoundException>(() => _service.DeleteAsync(UserId));

        ex.UserId.ShouldBe(UserId);
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Deleting_then_saving_again_starts_a_fresh_profile()
    {
        await _service.SaveAsync(UserId, Content());
        await _service.DeleteAsync(UserId);

        var dto = await _service.SaveAsync(UserId, Content(description: "De vuelta"));

        dto.Description.ShouldBe("De vuelta");
        _profiles.Added.Count.ShouldBe(2);   // a genuinely new row, not the old one revived
    }
}
