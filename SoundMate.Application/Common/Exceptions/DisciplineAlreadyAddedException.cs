namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// That person already studies this discipline.
/// <para>
/// A 409 rather than a quiet success, because this is a collection and "you already study piano"
/// is something the caller knows about themselves and wants to be told. The singleton profile of
/// issue #11 is the opposite case: there, whether a row existed was a persistence detail nobody
/// should have to ask about first.
/// </para>
/// <para>To change the level of one already there, <c>PUT</c> it.</para>
/// </summary>
public sealed class DisciplineAlreadyAddedException : Exception
{
    public DisciplineAlreadyAddedException(Guid userId, Guid disciplineId, Exception? innerException = null)
        : base($"User '{userId}' already studies discipline '{disciplineId}'. " +
               "Change the level with PUT instead.", innerException)
    {
        UserId = userId;
        DisciplineId = disciplineId;
    }

    public Guid UserId { get; }

    public Guid DisciplineId { get; }
}
