using Shouldly;
using SoundMate.Application.Disciplines;
using SoundMate.Application.Tests.Fakes;
using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;

namespace SoundMate.Application.Tests.Disciplines;

public class DisciplineServiceTests
{
    private readonly FakeDisciplineRepository _disciplines = new();
    private readonly DisciplineService _service;

    private readonly Discipline _piano;
    private readonly Discipline _organ;
    private readonly Discipline _guitar;
    private readonly Discipline _bandurria;

    public DisciplineServiceTests()
    {
        _piano = new Discipline(DisciplineId.New(), "Piano", DisciplineCategory.Keyboard);
        _organ = new Discipline(DisciplineId.New(), "Organ", DisciplineCategory.Keyboard);
        _guitar = new Discipline(DisciplineId.New(), "Classical guitar", DisciplineCategory.PluckedString);
        _bandurria = new Discipline(DisciplineId.New(), "Bandurria", DisciplineCategory.PluckedString, isActive: false);

        foreach (var discipline in new[] { _piano, _organ, _guitar, _bandurria })
            _disciplines.Seed(discipline);

        _service = new DisciplineService(_disciplines);
    }

    [Fact]
    public async Task Lists_the_whole_active_catalogue()
    {
        var listed = await _service.ListAsync();

        listed.Select(d => d.Name).ShouldBe(["Organ", "Piano", "Classical guitar"]);
    }

    [Fact]
    public async Task Never_offers_a_retired_discipline()
    {
        // IsActive is what stops something being offered without deleting it, so the selector must
        // not show it. The rows that already reference it keep working — that is the other half of
        // the rule, and it lives in StudiedDisciplineServiceTests.
        var listed = await _service.ListAsync();

        listed.ShouldNotContain(d => d.Name == "Bandurria");
    }

    [Fact]
    public async Task Filters_by_family()
    {
        var listed = await _service.ListAsync(DisciplineCategory.Keyboard);

        listed.Select(d => d.Name).ShouldBe(["Organ", "Piano"]);
        listed.ShouldAllBe(d => d.Category == DisciplineCategory.Keyboard);
    }

    [Fact]
    public async Task Carries_the_catalogue_id_a_caller_needs_to_send_back()
    {
        var listed = await _service.ListAsync(DisciplineCategory.Keyboard);

        listed.ShouldContain(d => d.Id == _piano.Id.Value);
    }

    [Fact]
    public async Task Refuses_a_family_outside_the_enum()
    {
        // Model binding lets any integer through as an enum, so this arrives perfectly typed and
        // meaningless. Filtering on it would answer 200 with an empty list and let the caller
        // believe the family is empty.
        await Should.ThrowAsync<DomainException>(() => _service.ListAsync((DisciplineCategory)99));
    }
}
