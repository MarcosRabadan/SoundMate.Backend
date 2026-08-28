using Shouldly;
using SoundMate.Application.Academies;
using SoundMate.Application.Academies.DTO;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Academies;

/// <summary>
/// The mapping is hand-written, so "a member was left unmapped" is a compile error rather than
/// something a test has to catch — <see cref="AcademyDto"/>'s members are <c>required</c>. What is
/// left to check is that each one is unwrapped correctly, which no compiler can tell us.
/// </summary>
public class AcademyMapperTests
{
    private static Academy AnAcademy(UserId? owner = null)
        => Academy.Create("Do Re Mi", AcademyType.Academy, Slug.Create("do-re-mi"), owner ?? UserId.New());

    [Fact]
    public void Unwraps_the_typed_ids_and_the_slug_value_object()
    {
        var owner = UserId.New();
        var academy = AnAcademy(owner);

        var dto = academy.ToDto();

        dto.Id.ShouldBe(academy.Id.Value);
        dto.OwnerUserId.ShouldBe(owner.Value);
        dto.Slug.ShouldBe("do-re-mi");
        dto.Name.ShouldBe("Do Re Mi");
        dto.CreatedAtUtc.ShouldBe(academy.CreatedAtUtc);
    }

    [Fact]
    public void Publishes_the_enums_by_name_not_by_number()
    {
        // The numeric values are a storage detail - explicit in the enum precisely so reordering
        // cannot corrupt data. The HTTP contract must not inherit them.
        var academy = Academy.Create("Ana", AcademyType.SoloTeacher, Slug.Create("ana"), UserId.New());

        var dto = academy.ToDto();

        dto.Type.ShouldBe(AcademyType.SoloTeacher);
        dto.Plan.ShouldBe(SubscriptionPlan.Free);
        dto.Status.ShouldBe(AcademyStatus.Active);
    }

    [Fact]
    public void Reflects_a_cancelled_status()
    {
        var academy = AnAcademy();
        academy.Cancel();

        academy.ToDto().Status.ShouldBe(AcademyStatus.Cancelled);
    }

    [Fact]
    public void Maps_a_whole_list()
    {
        var owner = UserId.New();
        Academy[] academies =
        [
            Academy.Create("Uno", AcademyType.Academy, Slug.Create("uno"), owner),
            Academy.Create("Dos", AcademyType.Academy, Slug.Create("dos"), owner)
        ];

        academies.ToDtos().Select(a => a.Slug).ShouldBe(["uno", "dos"]);
    }

    [Fact]
    public void Never_exposes_the_deletion_date()
    {
        // A deleted academy never reaches a response, so the field would be null in every one of
        // them - a column of nulls that invites someone to start relying on it.
        typeof(AcademyDto).GetProperty("DeletedAtUtc").ShouldBeNull();
    }
}
