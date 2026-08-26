using FluentValidation.TestHelper;
using SoundMate.Application.Users.DTO;
using SoundMate.Application.Users.Validators;

namespace SoundMate.Application.Tests.Users;

public class ChangePasswordDtoValidatorTests
{
    private readonly ChangePasswordDtoValidator _validator = new();

    private static ChangePasswordDto Valid() => new()
    {
        CurrentPassword = "Str0ngPass!",
        NewPassword = "An0therPass!"
    };

    [Fact]
    public void Accepts_a_well_formed_change()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Requires_the_current_password(string current)
        => _validator.TestValidate(Valid() with { CurrentPassword = current })
                     .ShouldHaveValidationErrorFor(x => x.CurrentPassword);

    [Fact]
    public void Does_not_apply_todays_length_rule_to_the_current_password()
    {
        // The current password is checked against the stored hash, not against the current rules.
        // Applying them would lock out anyone whose password predates a tightening — and tell
        // them why, which is worse.
        _validator.TestValidate(Valid() with { CurrentPassword = "old" })
                  .ShouldNotHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Applies_the_length_rule_to_the_new_password(string replacement)
        => _validator.TestValidate(Valid() with { NewPassword = replacement })
                     .ShouldHaveValidationErrorFor(x => x.NewPassword);

    [Fact]
    public void Rejects_a_change_that_changes_nothing()
        => _validator.TestValidate(new ChangePasswordDto
        {
            CurrentPassword = "Str0ngPass!",
            NewPassword = "Str0ngPass!"
        }).ShouldHaveValidationErrorFor(x => x.NewPassword);
}
