using FluentValidation;
using SoundMate.Application.Academies.DTO;

namespace SoundMate.Application.Academies.Validators;

public sealed class ChangePlanDtoValidator : AbstractValidator<ChangePlanDto>
{
    public ChangePlanDtoValidator()
    {
        RuleFor(x => x.Plan)
            .IsInEnum().WithMessage("Plan must be Free, Basic or Pro.");
    }
}
