using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;

namespace SoundMate.Domain.Tests.Disciplines;

public class DisciplineTests
{
    [Fact]
    public void Constructor_Valid_SetsFieldsAndActiveByDefault()
    {
        var id = DisciplineId.New();
        var discipline = new Discipline(id, "Piano", DisciplineCategory.Keyboard);

        discipline.Id.ShouldBe(id);
        discipline.Name.ShouldBe("Piano");
        discipline.Category.ShouldBe(DisciplineCategory.Keyboard);
        discipline.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyName_Throws(string? name)
        => Should.Throw<DomainException>(() => new Discipline(DisciplineId.New(), name!, DisciplineCategory.Keyboard));

    [Fact]
    public void Constructor_UndefinedCategory_Throws()
        => Should.Throw<DomainException>(() => new Discipline(DisciplineId.New(), "Piano", (DisciplineCategory)99));

    [Fact]
    public void Deactivate_ThenActivate_TogglesIsActive()
    {
        var discipline = new Discipline(DisciplineId.New(), "Piano", DisciplineCategory.Keyboard);

        discipline.Deactivate();
        discipline.IsActive.ShouldBeFalse();

        discipline.Activate();
        discipline.IsActive.ShouldBeTrue();
    }
}
