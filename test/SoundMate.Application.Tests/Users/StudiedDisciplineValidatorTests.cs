using Shouldly;
using SoundMate.Application.Users.DTO;
using SoundMate.Application.Users.Validators;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Users;

public class StudiedDisciplineValidatorTests
{
    private readonly AddStudiedDisciplineDtoValidator _add = new();
    private readonly ChangeLevelDtoValidator _change = new();

    [Fact]
    public void Accepts_a_catalogue_id_with_a_defined_level()
    {
        var dto = new AddStudiedDisciplineDto { DisciplineId = Guid.NewGuid(), Level = MusicLevel.Advanced };

        _add.Validate(dto).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Rejects_an_empty_discipline_id()
    {
        // Guid.Empty is what an absent or unparsed id deserialises to. Caught here it is a 400
        // naming the field; let through it would be a 404 about an id nobody sent.
        var dto = new AddStudiedDisciplineDto { DisciplineId = Guid.Empty, Level = MusicLevel.Advanced };

        var result = _add.Validate(dto);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AddStudiedDisciplineDto.DisciplineId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    public void Rejects_a_level_outside_the_enum(int level)
    {
        // MusicLevel has explicit values and a cast happily produces one that is not defined.
        // Without IsInEnum this reaches Guard.Defined in the aggregate and surfaces as a thrown
        // invariant instead of a 400 pointing at the field.
        var dto = new AddStudiedDisciplineDto { DisciplineId = Guid.NewGuid(), Level = (MusicLevel)level };

        _add.Validate(dto).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Names_every_level_in_the_message_it_gives_back()
    {
        // The message is built from the enum, so adding a level cannot leave it out of date.
        var dto = new AddStudiedDisciplineDto { DisciplineId = Guid.NewGuid(), Level = (MusicLevel)42 };

        var message = _add.Validate(dto).Errors.Single(e => e.PropertyName == nameof(AddStudiedDisciplineDto.Level))
            .ErrorMessage;

        foreach (var name in Enum.GetNames<MusicLevel>())
            message.ShouldContain(name);
    }

    [Fact]
    public void Accepts_a_defined_level_when_changing_it()
        => _change.Validate(new ChangeLevelDto { Level = MusicLevel.Superior }).IsValid.ShouldBeTrue();

    [Fact]
    public void Rejects_an_undefined_level_when_changing_it()
        => _change.Validate(new ChangeLevelDto { Level = (MusicLevel)42 }).IsValid.ShouldBeFalse();
}
