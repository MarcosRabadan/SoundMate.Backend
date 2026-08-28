using Shouldly;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Tests.Fakes;
using SoundMate.Application.Users;
using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Users;

public class StudiedDisciplineServiceTests
{
    private const string UniqueIndex = "IX_StudiedDisciplines_UserId_DisciplineId";

    private readonly FakeStudiedDisciplineRepository _studied = new();
    private readonly FakeDisciplineRepository _disciplines = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly StudiedDisciplineService _service;

    private readonly User _user;
    private readonly Discipline _piano;
    private readonly Discipline _guitar;

    /// <summary>Retired from the catalogue — the awkward case decision 2 of issue #13 is about.</summary>
    private readonly Discipline _bandurria;

    public StudiedDisciplineServiceTests()
    {
        _user = User.Register(Email.Create("ana@example.com"), "hash", "Ana García");
        _users.Seed(_user);

        _piano = new Discipline(DisciplineId.New(), "Piano", DisciplineCategory.Keyboard);
        _guitar = new Discipline(DisciplineId.New(), "Classical guitar", DisciplineCategory.PluckedString);
        _bandurria = new Discipline(DisciplineId.New(), "Bandurria", DisciplineCategory.PluckedString, isActive: false);

        _disciplines.Seed(_piano);
        _disciplines.Seed(_guitar);
        _disciplines.Seed(_bandurria);

        _service = new StudiedDisciplineService(_studied, _disciplines, _users, _unitOfWork);
    }

    private Guid UserId => _user.Id.Value;

    private static AddStudiedDisciplineDto Add(Discipline discipline, MusicLevel level = MusicLevel.Advanced)
        => new() { DisciplineId = discipline.Id.Value, Level = level };

    // ---------------------------------------------------------------- read

    [Fact]
    public async Task Lists_nothing_for_someone_who_studies_nothing()
        => (await _service.ListByUserAsync(UserId)).ShouldBeEmpty();

    [Fact]
    public async Task Resolves_the_catalogue_name_and_family_into_every_row()
    {
        await _service.AddAsync(UserId, Add(_piano));

        var listed = await _service.ListByUserAsync(UserId);

        var only = listed.ShouldHaveSingleItem();
        only.DisciplineId.ShouldBe(_piano.Id.Value);
        only.Name.ShouldBe("Piano");
        only.Category.ShouldBe(DisciplineCategory.Keyboard);
        only.Level.ShouldBe(MusicLevel.Advanced);
    }

    [Fact]
    public async Task Lists_by_family_and_then_by_name()
    {
        await _service.AddAsync(UserId, Add(_guitar, MusicLevel.Beginner));
        await _service.AddAsync(UserId, Add(_piano));

        var listed = await _service.ListByUserAsync(UserId);

        // Keyboard is 1 and PluckedString is 2, so the piano comes first however they went in.
        listed.Select(d => d.Name).ShouldBe(["Piano", "Classical guitar"]);
    }

    // ---------------------------------------------------------------- the user

    [Theory]
    [InlineData("list")]
    [InlineData("add")]
    [InlineData("change")]
    [InlineData("remove")]
    public async Task Refuses_a_user_that_does_not_exist(string operation)
        => await Should.ThrowAsync<UserNotFoundException>(() => Operate(operation, Guid.NewGuid()));

    [Theory]
    [InlineData("list")]
    [InlineData("add")]
    [InlineData("change")]
    [InlineData("remove")]
    public async Task Refuses_a_user_whose_account_is_deleted(string operation)
    {
        // The rows are still there; the person is not reachable. Same rule as their profile in #11.
        _studied.Seed(StudiedDiscipline.Create(_user.Id, _piano.Id, MusicLevel.Advanced));
        _user.Delete();

        await Should.ThrowAsync<UserNotFoundException>(() => Operate(operation, UserId));
    }

    private Task Operate(string operation, Guid userId) => operation switch
    {
        "list" => _service.ListByUserAsync(userId),
        "add" => _service.AddAsync(userId, Add(_guitar)),
        "change" => _service.ChangeLevelAsync(userId, _piano.Id.Value, Level(MusicLevel.Superior)),
        _ => _service.RemoveAsync(userId, _piano.Id.Value)
    };

    private static ChangeLevelDto Level(MusicLevel level) => new() { Level = level };

    // ---------------------------------------------------------------- add

    [Fact]
    public async Task Adds_a_discipline_and_returns_it_resolved()
    {
        var added = await _service.AddAsync(UserId, Add(_piano, MusicLevel.Intermediate));

        added.Name.ShouldBe("Piano");
        added.Level.ShouldBe(MusicLevel.Intermediate);
        added.CreatedAtUtc.ShouldBe(added.UpdatedAtUtc);
        _studied.Added.ShouldHaveSingleItem();
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Refuses_a_discipline_that_is_not_in_the_catalogue()
    {
        // Nothing at the database level stops this: aggregates reference each other by identity
        // and there is no cross-aggregate FK, so the check has to be here or not at all.
        var dto = new AddStudiedDisciplineDto { DisciplineId = Guid.NewGuid(), Level = MusicLevel.Beginner };

        await Should.ThrowAsync<DisciplineNotFoundException>(() => _service.AddAsync(UserId, dto));
    }

    [Fact]
    public async Task Refuses_to_take_up_a_discipline_retired_from_the_catalogue()
        => await Should.ThrowAsync<DisciplineNotAvailableException>(
            () => _service.AddAsync(UserId, Add(_bandurria)));

    [Fact]
    public async Task Refuses_a_discipline_already_studied()
    {
        await _service.AddAsync(UserId, Add(_piano));

        await Should.ThrowAsync<DisciplineAlreadyAddedException>(
            () => _service.AddAsync(UserId, Add(_piano, MusicLevel.Superior)));
    }

    [Fact]
    public async Task Answers_the_same_conflict_when_it_loses_the_race_against_the_index()
    {
        // ExistsAsync and AddAsync are two statements, so both callers pass the check and the
        // index rejects the loser. Unhandled that would be a 500 for the same "already there".
        _unitOfWork.FailWithUniqueViolationOn = UniqueIndex;

        await Should.ThrowAsync<DisciplineAlreadyAddedException>(() => _service.AddAsync(UserId, Add(_piano)));
    }

    [Fact]
    public async Task Does_not_swallow_a_unique_violation_from_another_index()
    {
        // Matching on the index name is what keeps a future second index on the table from being
        // reported to the caller as a duplicate discipline.
        _unitOfWork.FailWithUniqueViolationOn = "IX_StudiedDisciplines_SomethingElse";

        await Should.ThrowAsync<UniqueConstraintViolationException>(() => _service.AddAsync(UserId, Add(_piano)));
    }

    [Fact]
    public async Task Refuses_a_level_outside_the_enum()
    {
        var dto = new AddStudiedDisciplineDto { DisciplineId = _piano.Id.Value, Level = (MusicLevel)42 };

        await Should.ThrowAsync<DomainException>(() => _service.AddAsync(UserId, dto));
    }

    // ---------------------------------------------------------------- change level

    [Fact]
    public async Task Changes_the_level_and_moves_only_the_update_stamp()
    {
        var added = await _service.AddAsync(UserId, Add(_piano, MusicLevel.Beginner));

        var changed = await _service.ChangeLevelAsync(UserId, _piano.Id.Value, Level(MusicLevel.Professional));

        changed.Level.ShouldBe(MusicLevel.Professional);
        changed.CreatedAtUtc.ShouldBe(added.CreatedAtUtc);
        changed.UpdatedAtUtc.ShouldBeGreaterThanOrEqualTo(added.UpdatedAtUtc);
    }

    [Fact]
    public async Task Refuses_to_change_the_level_of_something_not_studied()
        => await Should.ThrowAsync<StudiedDisciplineNotFoundException>(
            () => _service.ChangeLevelAsync(UserId, _piano.Id.Value, Level(MusicLevel.Superior)));

    [Fact]
    public async Task Refuses_a_level_outside_the_enum_when_changing_it()
    {
        await _service.AddAsync(UserId, Add(_piano));

        await Should.ThrowAsync<DomainException>(
            () => _service.ChangeLevelAsync(UserId, _piano.Id.Value, Level((MusicLevel)42)));
    }

    // ------------------------------------------ decision 2: retired but already held

    [Fact]
    public async Task A_retired_discipline_already_held_still_lists_with_its_name()
    {
        // It was taken up while it was still on offer. The catalogue changing afterwards is not
        // this person's doing, and their level on it is still true.
        _studied.Seed(StudiedDiscipline.Create(_user.Id, _bandurria.Id, MusicLevel.Advanced));

        var only = (await _service.ListByUserAsync(UserId)).ShouldHaveSingleItem();

        only.Name.ShouldBe("Bandurria");
        only.Level.ShouldBe(MusicLevel.Advanced);
    }

    [Fact]
    public async Task A_retired_discipline_already_held_can_still_change_level()
    {
        _studied.Seed(StudiedDiscipline.Create(_user.Id, _bandurria.Id, MusicLevel.Advanced));

        var changed = await _service.ChangeLevelAsync(UserId, _bandurria.Id.Value, Level(MusicLevel.Superior));

        changed.Level.ShouldBe(MusicLevel.Superior);
    }

    // ---------------------------------------------------------------- remove

    [Fact]
    public async Task Removes_a_studied_discipline()
    {
        await _service.AddAsync(UserId, Add(_piano));

        await _service.RemoveAsync(UserId, _piano.Id.Value);

        (await _service.ListByUserAsync(UserId)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Refuses_to_remove_something_not_studied()
        => await Should.ThrowAsync<StudiedDisciplineNotFoundException>(
            () => _service.RemoveAsync(UserId, _guitar.Id.Value));
}
