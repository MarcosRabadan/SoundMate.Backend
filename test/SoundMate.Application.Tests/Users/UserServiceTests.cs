using Shouldly;
using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Tests.Fakes;
using SoundMate.Application.Users;
using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Users;

public class UserServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly UserService _service;

    public UserServiceTests() => _service = new UserService(_users, _unitOfWork, _hasher);

    private static RegisterUserDto Request(string email = "ana@example.com") => new()
    {
        Email = email,
        Password = "Str0ngPass!",
        FullName = "Ana García",
        Phone = "600123123"
    };

    [Fact]
    public async Task Registers_a_new_user_and_commits()
    {
        var dto = await _service.RegisterAsync(Request());

        dto.Email.ShouldBe("ana@example.com");
        dto.FullName.ShouldBe("Ana García");
        dto.Status.ShouldBe(nameof(UserStatus.Active));
        dto.Id.ShouldNotBe(Guid.Empty);

        _users.Added.Count.ShouldBe(1);
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Stores_the_hash_and_never_the_password()
    {
        await _service.RegisterAsync(Request());

        var stored = _users.Added.Single();

        stored.PasswordHash.ShouldNotBe("Str0ngPass!");
        stored.PasswordHash.ShouldStartWith(FakePasswordHasher.Prefix);
        _hasher.Verify("Str0ngPass!", stored.PasswordHash).ShouldBeTrue();
    }

    [Fact]
    public async Task Rejects_an_email_that_is_already_taken()
    {
        _users.Seed(User.Register(Email.Create("ana@example.com"), "hash", "Ana"));

        var ex = await Should.ThrowAsync<EmailAlreadyRegisteredException>(
            () => _service.RegisterAsync(Request()));

        ex.Email.ShouldBe("ana@example.com");

        // Nothing was written and nothing was committed.
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
            () => _service.RegisterAsync(Request("ANA@EXAMPLE.COM")));
    }

    [Fact]
    public async Task Answers_409_not_500_when_it_loses_the_race_against_the_unique_index()
    {
        // Two registrations for the same email in flight: both passed the existence check, and
        // the index rejected this one. Unhandled, Postgres' 23505 reaches the caller as a 500.
        _unitOfWork.FailWithUniqueViolationOn = "IX_Users_Email";

        var ex = await Should.ThrowAsync<EmailAlreadyRegisteredException>(
            () => _service.RegisterAsync(Request()));

        ex.Email.ShouldBe("ana@example.com");
        ex.InnerException.ShouldBeOfType<Abstractions.Persistence.UniqueConstraintViolationException>();
    }

    [Fact]
    public async Task Does_not_disguise_a_violation_of_some_other_index_as_a_duplicate_email()
    {
        // Users has one unique index today. If a second one is ever added, its violations must
        // keep surfacing as themselves rather than becoming a misleading "email taken".
        _unitOfWork.FailWithUniqueViolationOn = "IX_Users_SomethingElse";

        await Should.ThrowAsync<Abstractions.Persistence.UniqueConstraintViolationException>(
            () => _service.RegisterAsync(Request()));
    }

    [Fact]
    public async Task Rejects_a_malformed_email_before_touching_the_database()
    {
        await Should.ThrowAsync<DomainException>(() => _service.RegisterAsync(Request("not-an-email")));

        _users.Added.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Returns_null_for_an_unknown_id()
    {
        (await _service.GetByIdAsync(Guid.NewGuid())).ShouldBeNull();
    }

    [Fact]
    public async Task Finds_a_user_by_id()
    {
        var registered = await _service.RegisterAsync(Request());

        var found = await _service.GetByIdAsync(registered.Id);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(registered.Id);
        found.Email.ShouldBe("ana@example.com");
    }
}
