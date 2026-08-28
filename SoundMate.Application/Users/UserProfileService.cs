using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Users;

/// <inheritdoc cref="IUserProfileService"/>
internal sealed class UserProfileService : IUserProfileService
{
    /// <summary>
    /// The unique index behind "one profile per user". Mirrors <c>UserProfileConfiguration</c>,
    /// where <c>HasIndex(p => p.UserId).IsUnique()</c> makes EF generate this name.
    /// </summary>
    private const string UserUniqueIndex = "IX_UserProfiles_UserId";

    private readonly IUserProfileRepository _profiles;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public UserProfileService(IUserProfileRepository profiles,
                              IUserRepository users,
                              IUnitOfWork unitOfWork)
    {
        _profiles = profiles;
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto?> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetLiveUserAsync(userId, cancellationToken);

        var profile = await _profiles.GetByUserAsync(user.Id, cancellationToken);

        // null, not an exception: "this person has not written a bio" is an empty state a profile
        // page renders, not an error. The controller turns it into a 404 with that meaning.
        return profile?.ToDto();
    }

    public async Task<UserProfileDto> SaveAsync(Guid userId,
                                                UpdateUserProfileDto dto,
                                                CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await GetLiveUserAsync(userId, cancellationToken);

        var existing = await _profiles.GetByUserAsync(user.Id, cancellationToken);
        if (existing is not null)
            return await ApplyAndSaveAsync(existing, dto, cancellationToken);

        var profile = UserProfile.Create(user.Id);
        Apply(profile, dto);

        await _profiles.AddAsync(profile, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException ex) when (ex.ConstraintName == UserUniqueIndex)
        {
            // Two PUTs for the same brand-new profile at once: both read nothing, both inserted,
            // and the index rejected this one. Answering 409 would be wrong — a PUT says "make the
            // profile be this", and it now exists, so the honest thing is to apply the change to
            // the row that won. That leaves the verb idempotent even under a race, which is the
            // whole promise of PUT.
            //
            // Re-reading works because UnitOfWork detaches the entries a failed save left behind;
            // otherwise the next SaveChanges would replay the insert the index just rejected.
            var winner = await _profiles.GetByUserAsync(user.Id, cancellationToken);

            // The index said the row exists and it does not. That is not this race, so let the
            // original travel as itself rather than be swallowed by a recovery for another problem.
            if (winner is null)
                throw;

            return await ApplyAndSaveAsync(winner, dto, cancellationToken);
        }

        return profile.ToDto();
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetLiveUserAsync(userId, cancellationToken);

        var profile = await _profiles.GetByUserAsync(user.Id, cancellationToken)
                      ?? throw new UserProfileNotFoundException(userId);

        _profiles.Remove(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<UserProfileDto> ApplyAndSaveAsync(UserProfile profile,
                                                         UpdateUserProfileDto dto,
                                                         CancellationToken cancellationToken)
    {
        Apply(profile, dto);

        _profiles.Update(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return profile.ToDto();
    }

    /// <summary>
    /// Both fields, always. This is a PUT: what the body does not mention is not "left alone", it
    /// is absent, so sending only a description clears the avatar.
    /// </summary>
    private static void Apply(UserProfile profile, UpdateUserProfileDto dto)
    {
        // Avatar first: it is the one that can be refused, and doing it before the description
        // means a bad URL cannot leave half the change applied.
        profile.UpdateAvatar(dto.AvatarUrl);
        profile.UpdateDescription(dto.Description);
    }

    /// <summary>
    /// The user, or <see cref="UserNotFoundException"/> — a deleted one counts as missing, so a
    /// closed account cannot keep serving or growing a public profile.
    /// </summary>
    private async Task<User> GetLiveUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(UserId.From(userId), cancellationToken);

        return user is null || user.IsDeleted ? throw new UserNotFoundException(userId) : user;
    }
}
