using FluentValidation.TestHelper;
using SoundMate.Application.Users.DTO;
using SoundMate.Application.Users.Validators;

namespace SoundMate.Application.Tests.Users;

public class RegisterUserDtoValidatorTests
{
    private readonly RegisterUserDtoValidator _validator = new();

    private static RegisterUserDto Valid() => new()
    {
        Email = "ana@example.com",
        Password = "Str0ngPass!",
        FullName = "Ana García",
        Phone = "600123123"
    };

    [Fact]
    public void Accepts_a_well_formed_request()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Accepts_a_request_without_a_phone()
    {
        _validator.TestValidate(Valid() with { Phone = null }).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    public void Rejects_a_malformed_email(string email)
    {
        _validator.TestValidate(Valid() with { Email = email })
                  .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("1234567")]      // one short of the minimum
    public void Rejects_a_password_that_is_too_short(string password)
    {
        _validator.TestValidate(Valid() with { Password = password })
                  .ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Rejects_an_unbounded_password()
    {
        // Not a style rule: every attempt costs a full PBKDF2 derivation, so an unbounded
        // password is free CPU for whoever sends it.
        var tooLong = new string('a', UserRules.MaxPasswordLength + 1);

        _validator.TestValidate(Valid() with { Password = tooLong })
                  .ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_an_empty_name(string fullName)
    {
        _validator.TestValidate(Valid() with { FullName = fullName })
                  .ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Reports_every_bad_field_at_once_not_just_the_first()
    {
        // The whole point of validating here rather than letting the domain throw: the caller
        // gets one round trip instead of one per mistake.
        var result = _validator.TestValidate(new RegisterUserDto
        {
            Email = "nope",
            Password = "x",
            FullName = ""
        });

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }
}
