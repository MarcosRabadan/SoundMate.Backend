using FluentValidation;
using SoundMate.Application.Academies.DTO;

namespace SoundMate.Application.Academies.Validators;

public sealed class ChangeSlugDtoValidator : AbstractValidator<ChangeSlugDto>
{
    public ChangeSlugDtoValidator()
    {
        RuleFor(x => x.Slug).AcademySlug();
    }
}
