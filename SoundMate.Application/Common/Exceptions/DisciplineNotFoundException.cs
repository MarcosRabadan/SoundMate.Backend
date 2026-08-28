namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// No discipline in the catalogue has that id.
/// <para>
/// The domain cannot catch this: aggregates reference each other by identity and there is no
/// enforced cross-aggregate FK, so nothing at the database level stops a row pointing at a
/// discipline that was never seeded. Checking it here is what keeps that from happening.
/// </para>
/// </summary>
public sealed class DisciplineNotFoundException : Exception
{
    public DisciplineNotFoundException(Guid disciplineId)
        : base($"No discipline '{disciplineId}' in the catalogue.") => DisciplineId = disciplineId;

    public Guid DisciplineId { get; }
}
