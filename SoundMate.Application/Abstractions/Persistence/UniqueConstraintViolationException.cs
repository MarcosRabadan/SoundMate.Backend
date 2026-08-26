namespace SoundMate.Application.Abstractions.Persistence;

/// <summary>
/// A write lost a race against a unique index. Infrastructure translates the database's own error
/// into this one so the Application layer can react to it without referencing EF Core or Npgsql.
/// <para>
/// It matters because the alternative is a 500: the raw provider exception says nothing a caller
/// can act on, and "you lost a race" is usually the same answer as the check that ran a moment
/// earlier and passed.
/// </para>
/// </summary>
public sealed class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException(string? constraintName, Exception innerException)
        : base(BuildMessage(constraintName), innerException)
        => ConstraintName = constraintName;

    /// <summary>
    /// The index that rejected the write, e.g. <c>IX_Users_Email</c>. Null when the provider did
    /// not name it, so callers must handle that rather than assume.
    /// </summary>
    public string? ConstraintName { get; }

    private static string BuildMessage(string? constraintName) =>
        constraintName is null
            ? "A unique constraint was violated."
            : $"The unique constraint '{constraintName}' was violated.";
}
