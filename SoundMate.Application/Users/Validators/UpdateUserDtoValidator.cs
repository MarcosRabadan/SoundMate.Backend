using FluentValidation;
using SoundMate.Application.Users.DTO;

namespace SoundMate.Application.Users.Validators;

public sealed class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FullName).FullName();

        RuleFor(x => x.Phone).Phone();
    }
}
