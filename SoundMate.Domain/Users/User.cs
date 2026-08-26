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

    /// <summary>
    /// When the user was soft-deleted, or <c>null</c> while they are still here.
    /// <para>
    /// Deliberately <b>not</b> a <see cref="UserStatus"/> value. Suspension is a moderation call
    /// about a person who still exists; deletion is a lifecycle fact about the record. Folding
    /// them into one enum would mean deleting a suspended user forgets they were suspended, and
    /// restoring them would have to guess. As two independent facts, restore is exact.
    /// </para>
    /// <para>A date rather than a flag, because "when" is what a retention or purge policy needs.</para>
    /// </summary>
    public DateTime? DeletedAtUtc { get; private set; }

    /// <summary>True once <see cref="Delete"/> ran and <see cref="Restore"/> has not.</summary>
    public bool IsDeleted => DeletedAtUtc is not null;

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
        EnsureNotDeleted();
        FullName = Guard.NotNullOrWhiteSpace(fullName, "Full name");
        Touch();
    }

    public void ChangePhone(string? phone)
    {
        EnsureNotDeleted();
        Phone = Normalize(phone);
        Touch();
    }

    public void ChangePasswordHash(string passwordHash)
    {
        EnsureNotDeleted();
        PasswordHash = Guard.NotNullOrWhiteSpace(passwordHash, "Password hash");
        Touch();
    }

    public void VerifyEmail()
    {
        EnsureNotDeleted();
        EmailVerifiedAtUtc = DateTime.UtcNow;
        Touch();
    }

    public void Suspend()
    {
        EnsureNotDeleted();
        Status = UserStatus.Suspended;
        Touch();
    }

    public void Reactivate()
    {
        EnsureNotDeleted();
        Status = UserStatus.Active;
        Touch();
    }

    /// <summary>
    /// Soft-deletes the user. The row survives, which is the point: eight tables reference this
    /// <c>UserId</c> without a foreign key, so removing it for real would leave every one of them
    /// pointing at nothing.
    /// <para>Idempotent — deleting twice does not move the date.</para>
    /// </summary>
    public void Delete()
    {
        if (IsDeleted)
            return;

        DeletedAtUtc = DateTime.UtcNow;
        Touch();
    }

    /// <summary>
    /// Brings a soft-deleted user back exactly as they were, suspension included: deleting never
    /// touched <see cref="Status"/>, so there is nothing to reconstruct. Idempotent.
    /// </summary>
    public void Restore()
    {
        if (!IsDeleted)
            return;

        DeletedAtUtc = null;
        Touch();
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new DomainException("A deleted user cannot be modified. Restore them first.");
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

    private static string? Normalize(string? text)
        => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
