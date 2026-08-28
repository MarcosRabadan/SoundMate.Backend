using FluentValidation.TestHelper;
using Shouldly;
using SoundMate.Application.Academies.DTO;
using SoundMate.Application.Academies.Validators;
using SoundMate.Domain.Academies;

namespace SoundMate.Application.Tests.Academies;

public class CreateAcademyDtoValidatorTests
{
    private readonly CreateAcademyDtoValidator _validator = new();

    private static CreateAcademyDto Valid() => new()
    {
        Name = "Do Re Mi",
        Type = AcademyType.Academy,
        Slug = "do-re-mi",
        OwnerUserId = Guid.NewGuid()
    };

    [Fact]
    public void Accepts_a_well_formed_request()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_an_empty_name(string name)
        => _validator.TestValidate(Valid() with { Name = name })
                     .ShouldHaveValidationErrorFor(x => x.Name);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Mi Academia!")]   // spaces and punctuation
    [InlineData("-do-re-mi")]      // leading hyphen
    [InlineData("do--re")]         // doubled hyphen
    [InlineData("café")]           // non-ascii
    public void Rejects_a_malformed_slug(string slug)
    {
        // The rule comes from Slug.IsValid, not from a copy of the pattern living here: two
        // definitions of "valid slug" would eventually disagree, and the disagreement surfaces as
        // input that passes this 400 and then throws an invariant.
        _validator.TestValidate(Valid() with { Slug = slug })
                  .ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Accepts_a_slug_that_only_needs_normalising()
    {
        // Slug.Create trims and lowercases, so this is valid input, not a mistake to reject.
        _validator.TestValidate(Valid() with { Slug = "  Do-Re-Mi  " })
                  .ShouldNotHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Rejects_a_type_outside_the_enum()
        => _validator.TestValidate(Valid() with { Type = (AcademyType)99 })
                     .ShouldHaveValidationErrorFor(x => x.Type);

    [Fact]
    public void Requires_an_owner()
        => _validator.TestValidate(Valid() with { OwnerUserId = Guid.Empty })
                     .ShouldHaveValidationErrorFor(x => x.OwnerUserId);

    [Fact]
    public void Has_no_way_to_set_the_plan_at_creation_time()
    {
        // Every academy starts on Free. A caller picking their own plan is a billing decision
        // taken by whoever is holding the keyboard.
        typeof(CreateAcademyDto).GetProperty("Plan").ShouldBeNull();
    }
}
