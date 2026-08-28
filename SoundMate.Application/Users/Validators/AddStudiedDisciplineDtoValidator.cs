using FluentValidation;
using SoundMate.Application.Users.DTO;

namespace SoundMate.Application.Users.Validators;

public sealed class AddStudiedDisciplineDtoValidator : AbstractValidator<AddStudiedDisciplineDto>
{
    public AddStudiedDisciplineDtoValidator()
    {
        // Guid.Empty is what an absent or unparsed id deserialises to, and it can never match a
        // seeded discipline. Caught here it is a 400 naming the field; let through it is a 404
        // about an id the caller never sent.
        RuleFor(x => x.DisciplineId)
            .NotEmpty().WithMessage("Discipline id is required.");

        RuleFor(x => x.Level).Level();
    }
}
