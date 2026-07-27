using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Common;

public class GuardTests
{
    [Fact]
    public void NotNullOrWhiteSpace_Valid_ReturnsTrimmed()
        => Guard.NotNullOrWhiteSpace("  hello  ", "Field").ShouldBe("hello");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotNullOrWhiteSpace_Empty_Throws(string? input)
        => Should.Throw<DomainException>(() => Guard.NotNullOrWhiteSpace(input, "Field"));

    [Fact]
    public void NotEmpty_ValidId_ReturnsId()
    {
        var id = UserId.New();
        Guard.NotEmpty(id, "User").ShouldBe(id);
    }

    [Fact]
    public void NotEmpty_DefaultId_Throws()
        => Should.Throw<DomainException>(() => Guard.NotEmpty(default(UserId), "User"));

    [Fact]
    public void Defined_ValidEnum_ReturnsValue()
        => Guard.Defined(MusicLevel.Advanced, "Level").ShouldBe(MusicLevel.Advanced);

    [Fact]
    public void Defined_UndefinedEnum_Throws()
        => Should.Throw<DomainException>(() => Guard.Defined((MusicLevel)99, "Level"));

    [Fact]
    public void InRange_WithinBounds_ReturnsValue()
        => Guard.InRange(3, 1, 5, "Stars").ShouldBe(3);

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void InRange_OutOfBounds_Throws(int value)
        => Should.Throw<DomainException>(() => Guard.InRange(value, 1, 5, "Stars"));
}
