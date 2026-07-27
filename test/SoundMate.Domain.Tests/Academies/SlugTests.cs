using Shouldly;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Common;

namespace SoundMate.Domain.Tests.Academies;

public class SlugTests
{
    [Theory]
    [InlineData("do-re-mi")]
    [InlineData("academia123")]
    [InlineData("piano")]
    public void Create_WithValidInput_ReturnsSlug(string input)
        => Slug.Create(input).Value.ShouldBe(input);

    [Fact]
    public void Create_LowercasesAndTrims()
        => Slug.Create("  Do-Re-Mi  ").Value.ShouldBe("do-re-mi");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyInput_Throws(string? input)
        => Should.Throw<DomainException>(() => Slug.Create(input!));

    [Fact]
    public void Create_ExceedingMaxLength_Throws()
        => Should.Throw<DomainException>(() => Slug.Create(new string('a', 101)));

    [Theory]
    [InlineData("-abc")]      // leading hyphen
    [InlineData("abc-")]      // trailing hyphen
    [InlineData("a--b")]      // double hyphen
    [InlineData("a b")]       // space
    [InlineData("a_b")]       // underscore
    [InlineData("café")]      // non-ascii
    public void Create_WithInvalidChars_Throws(string input)
        => Should.Throw<DomainException>(() => Slug.Create(input));

    [Fact]
    public void Equality_SameValue_AreEqual()
        => Slug.Create("piano").ShouldBe(Slug.Create("piano"));
}
