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

    [Theory]
    [InlineData("do-re-mi")]
    [InlineData("  Do-Re-Mi  ")]   // trimmed and lowercased
    [InlineData("piano2")]
    public void IsValid_WithAcceptableInput_IsTrue(string input)
        => Slug.IsValid(input).ShouldBeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-abc")]
    [InlineData("abc-")]
    [InlineData("a--b")]
    [InlineData("a b")]
    [InlineData("a_b")]
    [InlineData("café")]
    public void IsValid_WithUnacceptableInput_IsFalse(string? input)
        => Slug.IsValid(input).ShouldBeFalse();

    [Fact]
    public void IsValid_ExceedingMaxLength_IsFalse()
        => Slug.IsValid(new string('a', Slug.MaxLength + 1)).ShouldBeFalse();

    [Theory]
    [InlineData("do-re-mi")]
    [InlineData("Do-Re-Mi")]
    [InlineData("a--b")]
    [InlineData("café")]
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_AgreesWithCreate(string? input)
    {
        // The whole reason IsValid exists: a validator that asks must get the same answer the
        // aggregate enforces. If these two drift, input passes the 400 and then throws an
        // invariant - which is exactly the bug this pins.
        var createSucceeds = true;
        try
        {
            Slug.Create(input!);
        }
        catch (DomainException)
        {
            createSucceeds = false;
        }

        Slug.IsValid(input).ShouldBe(createSucceeds);
    }
}
