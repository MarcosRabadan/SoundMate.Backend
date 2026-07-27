using SoundMate.Domain.Common;

namespace SoundMate.Domain.Users;

/// <summary>
/// The person: unique and global across all of SoundMate. The role they play in each place
/// is NOT here (that is a <c>Membership</c>), and neither is their musical skill (that is
/// <c>UserDiscipline</c>). Created and mutated only through its own methods, so it can never
/// exist without an email or a name.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    public Email Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string? Phone { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime? EmailVerifiedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private User() { }

    private User(UserId id, Email email, string passwordHash, string fullName, string? phone) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Phone = phone;
        Status = UserStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    /// <summary>Registers a new person. The password hash is computed outside the domain.</summary>
    public static User Register(Email email, string passwordHash, string fullName, string? phone = null)
    {
        ArgumentNullException.ThrowIfNull(email);

        return new User(
            UserId.New(),
            email,
            Guard.NotNullOrWhiteSpace(passwordHash, "Password hash"),
            Guard.NotNullOrWhiteSpace(fullName, "Full name"),
            Normalize(phone));
    }

    public void Rename(string fullName)
    {
        FullName = Guard.NotNullOrWhiteSpace(fullName, "Full name");
        Touch();
    }

    public void ChangePhone(string? phone)
    {
        Phone = Normalize(phone);
        Touch();
    }

    public void ChangePasswordHash(string passwordHash)
    {
        PasswordHash = Guard.NotNullOrWhiteSpace(passwordHash, "Password hash");
        Touch();
    }

    public void VerifyEmail()
    {
        EmailVerifiedAtUtc = DateTime.UtcNow;
        Touch();
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
        Touch();
    }

    public void Reactivate()
    {
        Status = UserStatus.Active;
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

    private static string? Normalize(string? text)
        => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
