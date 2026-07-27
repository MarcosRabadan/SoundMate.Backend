using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Genres;

namespace SoundMate.Domain.Tests.Genres;

public class GenreTests
{
    [Fact]
    public void Constructor_Valid_SetsFieldsAndActiveByDefault()
    {
        var id = GenreId.New();
        var genre = new Genre(id, "Flamenco");

        genre.Id.ShouldBe(id);
        genre.Name.ShouldBe("Flamenco");
        genre.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyName_Throws(string? name)
        => Should.Throw<DomainException>(() => new Genre(GenreId.New(), name!));

    [Fact]
    public void Deactivate_ThenActivate_TogglesIsActive()
    {
        var genre = new Genre(GenreId.New(), "Jazz");

        genre.Deactivate();
        genre.IsActive.ShouldBeFalse();

        genre.Activate();
        genre.IsActive.ShouldBeTrue();
    }
}
