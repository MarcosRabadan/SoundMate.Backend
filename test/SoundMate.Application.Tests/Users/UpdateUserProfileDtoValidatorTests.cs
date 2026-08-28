using FluentValidation.TestHelper;
using Shouldly;
using SoundMate.Application.Users.DTO;
using SoundMate.Application.Users.Validators;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Users;

public class UpdateUserProfileDtoValidatorTests
{
    private readonly UpdateUserProfileDtoValidator _validator = new();

    [Fact]
    public void Accepts_a_filled_in_profile()
        => _validator.TestValidate(new UpdateUserProfileDto
        {
            Description = "Profesora de piano",
            AvatarUrl = "https://cdn.example.com/ana.png"
        }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Accepts_an_entirely_empty_profile()
    {
        // Having a profile with nothing in it is a legitimate state, not a malformed request.
        _validator.TestValidate(new UpdateUserProfileDto()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("banana")]
    [InlineData("/avatars/ana.png")]
    [InlineData("ftp://cdn.example.com/ana.png")]
    [InlineData("javascript:alert(1)")]
    public void Rejects_an_avatar_that_is_not_an_absolute_http_url(string avatar)
    {
        // The rule comes from UserProfile.IsValidAvatarUrl, not from a URL regex written here: two
        // definitions would drift, and the drift reaches the caller as a thrown invariant instead
        // of this 400.
        _validator.TestValidate(new UpdateUserProfileDto { AvatarUrl = avatar })
                  .ShouldHaveValidationErrorFor(x => x.AvatarUrl);
    }

    [Fact]
    public void Rejects_a_description_longer_than_the_column()
        => _validator.TestValidate(new UpdateUserProfileDto
        {
            Description = new string('a', UserProfile.MaxDescriptionLength + 1)
        }).ShouldHaveValidationErrorFor(x => x.Description);

    [Fact]
    public void Rejects_an_avatar_url_longer_than_the_column()
        => _validator.TestValidate(new UpdateUserProfileDto
        {
            AvatarUrl = "https://cdn.example.com/" + new string('a', UserProfile.MaxAvatarUrlLength)
        }).ShouldHaveValidationErrorFor(x => x.AvatarUrl);

    [Fact]
    public void Has_no_way_to_set_the_user_or_the_profile_id()
    {
        // The user comes from the route. A body that also carried an id would let a caller rewrite
        // somebody else's bio by disagreeing with the URL.
        var members = typeof(UpdateUserProfileDto).GetProperties().Select(p => p.Name).ToArray();

        members.ShouldBe(["Description", "AvatarUrl"], ignoreOrder: true);
    }
}
