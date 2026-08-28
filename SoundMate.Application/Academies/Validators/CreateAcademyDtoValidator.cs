using FluentValidation;
using SoundMate.Application.Academies.DTO;

namespace SoundMate.Application.Academies.Validators;

public sealed class CreateAcademyDtoValidator : AbstractValidator<CreateAcademyDto>
{
    public CreateAcademyDtoValidator()
    {
        RuleFor(x => x.Name).AcademyName();

        RuleFor(x => x.Slug).AcademySlug();

        // IsInEnum, not the domain's Guard.Defined: an out-of-range value is a malformed request,
        // and the caller deserves a 400 naming the field rather than a thrown invariant.
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Academy type must be Academy or SoloTeacher.");

        RuleFor(x => x.OwnerUserId)
            .NotEmpty().WithMessage("The owner's user id is required.");
    }
}
