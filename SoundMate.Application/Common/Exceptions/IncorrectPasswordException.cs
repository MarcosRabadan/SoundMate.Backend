namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// The current password supplied to a password change did not match.
/// <para>
/// Changing a password without proving you know the current one is an account-takeover primitive,
/// not a convenience. That check is the only thing standing between "I can reach this endpoint"
/// and "I own this account" — especially now, while SoundMate has no authentication at all.
/// </para>
/// </summary>
public sealed class IncorrectPasswordException : Exception
{
    public IncorrectPasswordException()
        : base("The current password is incorrect.") { }
}
