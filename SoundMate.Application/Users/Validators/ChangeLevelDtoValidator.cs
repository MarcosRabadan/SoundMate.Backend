using FluentValidation;
using SoundMate.Application.Users.DTO;

namespace SoundMate.Application.Users.Validators;

public sealed class ChangeLevelDtoValidator : AbstractValidator<ChangeLevelDto>
{
    public ChangeLevelDtoValidator() => RuleFor(x => x.Level).Level();
}
