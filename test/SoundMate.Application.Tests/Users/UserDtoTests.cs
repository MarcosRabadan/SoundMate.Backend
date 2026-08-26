using Shouldly;
using SoundMate.Application.Users.DTO;

namespace SoundMate.Application.Tests.Users;

/// <summary>
/// Pins the shape of <see cref="UserDto"/>.
/// <para>
/// This is not busywork. AutoMapper fills members by convention, so adding a property called
/// <c>PasswordHash</c> to the DTO would start serialising every user's hash to every caller
/// without a single line of code being written to do it — and nothing else in the codebase would
/// complain. Here it fails a test instead, and whoever adds a legitimate new field has to say so
/// out loud by updating this list.
/// </para>
/// </summary>
public class UserDtoTests
{
    private static readonly string[] ExpectedProperties =
    [
        nameof(UserDto.Id),
        nameof(UserDto.Email),
        nameof(UserDto.FullName),
        nameof(UserDto.Phone),
        nameof(UserDto.Status),
        nameof(UserDto.CreatedAtUtc)
    ];

    [Fact]
    public void Exposes_exactly_the_agreed_fields_and_no_others()
    {
        var actual = typeof(UserDto)
            .GetProperties()
            .Select(p => p.Name)
            .Where(name => name != "EqualityContract")   // compiler-generated on every record
            .OrderBy(name => name)
            .ToArray();

        actual.ShouldBe(ExpectedProperties.OrderBy(name => name).ToArray(),
            "UserDto changed shape. If the new field is intentional, add it to ExpectedProperties " +
            "- and make very sure it is not a secret.");
    }

    [Theory]
    [InlineData("password")]
    [InlineData("hash")]
    [InlineData("secret")]
    [InlineData("token")]
    public void Never_exposes_anything_that_looks_like_a_credential(string forbiddenFragment)
    {
        var offenders = typeof(UserDto)
            .GetProperties()
            .Where(p => p.Name.Contains(forbiddenFragment, StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Name)
            .ToArray();

        offenders.ShouldBeEmpty($"UserDto must not carry credentials. Offending member(s): " +
                                $"{string.Join(", ", offenders)}");
    }
}
