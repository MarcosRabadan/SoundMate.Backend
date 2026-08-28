using FluentValidation;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Users.Validators;

/// <summary>
/// The rules shared by adding a studied discipline and changing its level, so the two cannot drift
/// into disagreeing about what a valid level is.
/// </summary>
internal static class StudiedDisciplineRuleExtensions
{
    /// <summary>
    /// Built from the enum rather than typed out, so adding a level updates the message with it.
    /// </summary>
    private static readonly string LevelMessage =
        $"Level must be one of: {string.Join(", ", Enum.GetNames<MusicLevel>())}.";

    /// <summary>
    /// <c>IsInEnum</c>, not a range check: <c>MusicLevel</c> has explicit values and an int cast
    /// happily produces one that is not defined. This is the same thing <c>Guard.Defined</c>
    /// enforces in the aggregate, caught early enough to name the field.
    /// </summary>
    public static IRuleBuilderOptions<T, MusicLevel> Level<T>(this IRuleBuilderInitial<T, MusicLevel> rule) =>
        rule.IsInEnum().WithMessage(LevelMessage);
}
