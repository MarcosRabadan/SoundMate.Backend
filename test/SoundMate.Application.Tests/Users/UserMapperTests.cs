using Shouldly;
using SoundMate.Application.Users;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Users;

/// <summary>
/// The mapping is hand-written, so "a member was left unmapped" is a compile error rather than
/// something a test has to catch — <c>UserDto</c>'s members are <c>required</c>. What is left to
/// check is that each one is unwrapped correctly, which no compiler can tell us.
/// </summary>
public class UserMapperTests
{
    [Fact]
    public void Unwraps_the_typed_id_and_the_email_value_object()
    {
        var user = User.Register(Email.Create("ana@example.com"), "irrelevant-hash", "Ana García", "600123123");

        var dto = user.ToDto();

        dto.Id.ShouldBe(user.Id.Value);
        dto.Email.ShouldBe("ana@example.com");
        dto.FullName.ShouldBe("Ana García");
        dto.Phone.ShouldBe("600123123");
        dto.CreatedAtUtc.ShouldBe(user.CreatedAtUtc);
    }

    [Fact]
    public void Keeps_a_missing_phone_missing()
    {
        var user = User.Register(Email.Create("ana@example.com"), "irrelevant-hash", "Ana García");

        user.ToDto().Phone.ShouldBeNull();
    }

    [Theory]
    [InlineData(false, "Active")]
    [InlineData(true, "Suspended")]
    public void Publishes_the_status_by_name_not_by_number(bool suspend, string expected)
    {
        // The enum's numbers are a storage detail — UserConfiguration persists them as int with
        // explicit values. The HTTP contract must not inherit that.
        var user = User.Register(Email.Create("ana@example.com"), "irrelevant-hash", "Ana García");
        if (suspend) user.Suspend();

        user.ToDto().Status.ShouldBe(expected);
    }

    [Fact]
    public void Preserves_the_email_exactly_as_stored_not_normalised()
    {
        // Normalized (upper case) is for comparison only; what goes back to the caller is what
        // they typed, so a confirmation email does not arrive shouting.
        var user = User.Register(Email.Create("Ana@Example.com"), "irrelevant-hash", "Ana García");

        user.ToDto().Email.ShouldBe("Ana@Example.com");
    }
}
