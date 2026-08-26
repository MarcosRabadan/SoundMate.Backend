namespace SoundMate.Application.Users.Validators;

/// <summary>
/// Limits shared by every validator that touches a user, so registering and updating cannot drift
/// into disagreeing about what a valid name or password is.
/// </summary>
public static class UserRules
{
    /// <summary>Short enough to be memorable, long enough not to be guessable.</summary>
    public const int MinPasswordLength = 8;

    /// <summary>
    /// PBKDF2 has no practical input limit, but an unbounded password is free CPU for whoever
    /// sends it: every attempt costs a full key derivation.
    /// </summary>
    public const int MaxPasswordLength = 128;

    // These mirror the column widths in UserConfiguration. Duplicated on purpose: caught here it
    // is a 400 naming the offending field, caught there it is a database error.
    public const int MaxFullNameLength = 200;
    public const int MaxPhoneLength = 30;
}
