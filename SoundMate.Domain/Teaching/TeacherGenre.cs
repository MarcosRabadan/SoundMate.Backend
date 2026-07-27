using SoundMate.Domain.Common;
using SoundMate.Domain.Genres;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Teaching;

/// <summary>
/// A genre the teacher plays/teaches (global to the person). Together with
/// <c>TeacherDiscipline</c> it makes up the teacher's derived "specialty" (e.g. electric
/// guitar + metal/rock).
/// </summary>
public sealed class TeacherGenre : AggregateRoot<TeacherGenreId>
{
    public UserId UserId { get; private set; }
    public GenreId GenreId { get; private set; }

    private TeacherGenre() { }

    private TeacherGenre(TeacherGenreId id, UserId userId, GenreId genreId) : base(id)
    {
        UserId = userId;
        GenreId = genreId;
    }

    public static TeacherGenre Create(UserId userId, GenreId genreId)
        => new(
            TeacherGenreId.New(),
            Guard.NotEmpty(userId, "User"),
            Guard.NotEmpty(genreId, "Genre"));
}
