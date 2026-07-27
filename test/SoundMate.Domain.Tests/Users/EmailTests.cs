using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Users;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ReturnsEmail()
        => Email.Create("ana@mail.com").Value.ShouldBe("ana@mail.com");

    [Fact]
    public void Create_TrimsSurroundingWhitespace()
        => Email.Create("  ana@mail.com  ").Value.ShouldBe("ana@mail.com");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyInput_Throws(string? input)
        => Should.Throw<DomainException>(() => Email.Create(input!));

    [Fact]
    public void Create_ExceedingMaxLength_Throws()
    {
        var tooLong = new string('a', 250) + "@mail.com";
        Should.Throw<DomainException>(() => Email.Create(tooLong));
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("no-at-sign.com")]
    [InlineData("a@b")]
    [InlineData("@mail.com")]
    [InlineData("ana@")]
    [InlineData("ana @mail.com")]
    [InlineData("ana@mail .com")]
    public void Create_WithInvalidFormat_Throws(string input)
        => Should.Throw<DomainException>(() => Email.Create(input));

    [Fact]
    public void Normalized_IsUpperCase()
        => Email.Create("Ana@Mail.com").Normalized.ShouldBe("ANA@MAIL.COM");

    [Fact]
    public void Equality_IsCaseInsensitive()
        => Email.Create("ana@mail.com").ShouldBe(Email.Create("ANA@MAIL.COM"));

    [Fact]
    public void Equality_DifferentEmails_AreNotEqual()
        => Email.Create("ana@mail.com").ShouldNotBe(Email.Create("luis@mail.com"));
}
