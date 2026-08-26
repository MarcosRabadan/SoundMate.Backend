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

    [Theory]
    [InlineData("ana@mail.com")]
    [InlineData("  ana@mail.com  ")]
    public void IsValid_WithAcceptableInput_IsTrue(string input)
        => Email.IsValid(input).ShouldBeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("plainaddress")]
    [InlineData("no-at-sign.com")]
    [InlineData("a@b")]
    [InlineData("@mail.com")]
    [InlineData("ana@")]
    [InlineData("ana @mail.com")]
    [InlineData("ana@mail .com")]
    public void IsValid_WithUnacceptableInput_IsFalse(string? input)
        => Email.IsValid(input).ShouldBeFalse();

    [Fact]
    public void IsValid_ExceedingMaxLength_IsFalse()
        => Email.IsValid(new string('a', 250) + "@mail.com").ShouldBeFalse();

    [Theory]
    [InlineData("ana@mail.com")]
    [InlineData("a@b")]
    [InlineData("ana@")]
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_AgreesWithCreate(string? input)
    {
        // The whole reason IsValid exists: callers that ask must get the same answer the
        // aggregate enforces. If these two ever drift, input passes validation and then fails
        // construction - which is exactly the bug this pins.
        var createSucceeds = true;
        try
        {
            Email.Create(input!);
        }
        catch (DomainException)
        {
            createSucceeds = false;
        }

        Email.IsValid(input).ShouldBe(createSucceeds);
    }

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
