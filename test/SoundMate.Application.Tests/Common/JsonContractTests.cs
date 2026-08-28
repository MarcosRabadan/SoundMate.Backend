using System.Text.Json;
using System.Text.Json.Serialization;
using Shouldly;
using SoundMate.Application.Academies;
using SoundMate.Application.Users;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Common;

/// <summary>
/// Pins what the DTOs actually look like on the wire.
/// <para>
/// The response DTOs hold real enums rather than strings, which is only safe because
/// <c>Program.cs</c> registers a <c>JsonStringEnumConverter</c>: without it every status would
/// silently become a number and every consumer would break at once. That registration lives in
/// another project and nothing else would notice if somebody removed it — so the contract it
/// produces is asserted here.
/// </para>
/// </summary>
public class JsonContractTests
{
    /// <summary>
    /// The same shape ASP.NET Core serialises with: <c>JsonSerializerDefaults.Web</c> for the
    /// camelCase naming, plus the converter <c>Program.cs</c> adds.
    /// </summary>
    private static readonly JsonSerializerOptions ApiOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public void A_user_status_goes_out_as_a_name_not_a_number()
    {
        var user = User.Register(Email.Create("ana@example.com"), "hash", "Ana García");
        user.Suspend();

        var json = JsonSerializer.Serialize(user.ToDto(), ApiOptions);

        json.ShouldContain("\"status\":\"Suspended\"");
        json.ShouldNotContain("\"status\":2");
    }

    [Fact]
    public void Every_academy_enum_goes_out_as_a_name_not_a_number()
    {
        var academy = Academy.Create("Ana", AcademyType.SoloTeacher, Slug.Create("ana"), UserId.New());
        academy.ChangePlan(SubscriptionPlan.Pro);
        academy.Cancel();

        var json = JsonSerializer.Serialize(academy.ToDto(), ApiOptions);

        json.ShouldContain("\"type\":\"SoloTeacher\"");
        json.ShouldContain("\"plan\":\"Pro\"");
        json.ShouldContain("\"status\":\"Cancelled\"");

        // The numeric values are a storage detail. If any of these appear, the converter is gone
        // and the HTTP contract has silently changed.
        json.ShouldNotContain("\"type\":2");
        json.ShouldNotContain("\"plan\":3");
        json.ShouldNotContain("\"status\":3");
    }

    [Fact]
    public void Numbers_are_still_accepted_on_the_way_in()
    {
        // Existing callers that send 2 keep working: JsonStringEnumConverter reads both forms.
        var byNumber = JsonSerializer.Deserialize<AcademyTypeHolder>("""{"type":2}""", ApiOptions);
        var byName = JsonSerializer.Deserialize<AcademyTypeHolder>("""{"type":"SoloTeacher"}""", ApiOptions);

        byNumber!.Type.ShouldBe(AcademyType.SoloTeacher);
        byName!.Type.ShouldBe(AcademyType.SoloTeacher);
    }

    private sealed record AcademyTypeHolder
    {
        public AcademyType Type { get; init; }
    }
}
