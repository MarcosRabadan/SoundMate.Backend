using FluentValidation;
using SoundMate.Application.Academies.DTO;

namespace SoundMate.Application.Academies.Validators;

public sealed class UpdateAcademyDtoValidator : AbstractValidator<UpdateAcademyDto>
{
    public UpdateAcademyDtoValidator()
    {
        RuleFor(x => x.Name).AcademyName();
    }
}
