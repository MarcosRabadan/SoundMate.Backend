using FluentValidation;
using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Users.Validators;

public sealed class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileDtoValidator()
    {
        // No NotEmpty on either: an empty profile is a legitimate state, and null is how a field
        // gets cleared.
        RuleFor(x => x.Description)
            .MaximumLength(UserProfile.MaxDescriptionLength);

        // UserProfile.IsValidAvatarUrl, not a URL regex written here. Same reasoning as
        // Email.IsValid and Slug.IsValid: a validator that restates a domain rule in its own words
        // eventually disagrees with it, and the disagreement reaches the caller as a thrown
        // invariant instead of the 400 this validator exists to give them.
        RuleFor(x => x.AvatarUrl)
            .Must(UserProfile.IsValidAvatarUrl)
            .WithMessage("Avatar URL must be an absolute http or https URL " +
                         $"of at most {UserProfile.MaxAvatarUrlLength} characters.");
    }
}
