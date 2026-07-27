using SoundMate.Domain.Common;

namespace SoundMate.Domain.Users;

/// <summary>
/// One qualification in a user's education section ("Bachelor's in Music, Conservatory of
/// Granada, 2015–2019"). A user has many. The year range is open: a null <see cref="EndYear"/>
/// means ongoing, and the end can never be before the start.
/// </summary>
public sealed class UserEducation : AggregateRoot<UserEducationId>
{
    public UserId UserId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Institution { get; private set; }
    public int? StartYear { get; private set; }
    public int? EndYear { get; private set; }
    public string? Description { get; private set; }

    private UserEducation() { }

    private UserEducation(UserEducationId id, UserId userId) : base(id) => UserId = userId;

    public static UserEducation Create(
        UserId userId,
        string title,
        string? institution = null,
        int? startYear = null,
        int? endYear = null,
        string? description = null)
    {
        var education = new UserEducation(UserEducationId.New(), Guard.NotEmpty(userId, "User"));
        education.SetDetails(title, institution, startYear, endYear, description);
        return education;
    }

    public void Update(string title, string? institution, int? startYear, int? endYear, string? description)
        => SetDetails(title, institution, startYear, endYear, description);

    private void SetDetails(string title, string? institution, int? startYear, int? endYear, string? description)
    {
        if (startYear.HasValue && endYear.HasValue && endYear < startYear)
            throw new DomainException("End year cannot be before start year.");

        Title = Guard.NotNullOrWhiteSpace(title, "Title");
        Institution = Normalize(institution);
        StartYear = startYear;
        EndYear = endYear;
        Description = Normalize(description);
    }

    private static string? Normalize(string? text)
        => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
