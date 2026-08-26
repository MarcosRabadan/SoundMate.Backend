using FluentValidation.TestHelper;
using Shouldly;
using SoundMate.Application.Users.DTO;
using SoundMate.Application.Users.Validators;

namespace SoundMate.Application.Tests.Users;

public class UpdateUserDtoValidatorTests
{
    private readonly UpdateUserDtoValidator _validator = new();

    [Fact]
    public void Accepts_a_name_and_a_phone()
        => _validator.TestValidate(new UpdateUserDto { FullName = "Ana García", Phone = "600123123" })
                     .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Accepts_a_null_phone_because_that_is_how_you_clear_it()
        => _validator.TestValidate(new UpdateUserDto { FullName = "Ana García", Phone = null })
                     .ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_an_empty_name(string fullName)
        => _validator.TestValidate(new UpdateUserDto { FullName = fullName })
                     .ShouldHaveValidationErrorFor(x => x.FullName);

    [Fact]
    public void Rejects_a_phone_longer_than_the_column()
        => _validator.TestValidate(new UpdateUserDto
        {
            FullName = "Ana García",
            Phone = new string('9', UserRules.MaxPhoneLength + 1)
        }).ShouldHaveValidationErrorFor(x => x.Phone);

    [Fact]
    public void Has_no_way_to_change_the_email_or_the_password()
    {
        // Not a style preference: an Update DTO that carried the id or the email would let a
        // caller edit somebody else by disagreeing with the route, or take over an account by
        // moving its identity. The type simply does not offer the fields.
        var members = typeof(UpdateUserDto).GetProperties().Select(p => p.Name).ToArray();

        members.ShouldBe(["FullName", "Phone"], ignoreOrder: true);
    }
}
