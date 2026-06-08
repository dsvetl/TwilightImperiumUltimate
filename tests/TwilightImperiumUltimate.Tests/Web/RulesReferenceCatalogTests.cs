using FluentAssertions;
using System.Text.Json;
using TwilightImperiumUltimate.Web.Components.Library.Models;
using TwilightImperiumUltimate.Web.Components.Rules.Reference;
using Xunit;

namespace TwilightImperiumUltimate.Tests.Web;

public class RulesReferenceCatalogTests
{
    [Fact]
    public async Task GeneratedCatalogContainsLegacyAndThundersEdgeTopics()
    {
        var path = DataPath("rules-reference.json");
        await using var stream = File.OpenRead(path);
        var catalog = await JsonSerializer.DeserializeAsync<RulesReferenceCatalog>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        catalog.Should().NotBeNull();
        catalog!.Rules.Should().HaveCount(109);
        catalog.Rules.Should().Contain(rule =>
            rule.LegacyId == 36 && rule.Key == "fighter-tokens");
        catalog.Rules.Should().Contain(rule =>
            rule.LegacyId == null && rule.Key == "breakthroughs");
        catalog.Rules.Should().Contain(rule =>
            rule.LegacyId == null && rule.Key == "fracture");
        catalog.Sources.Should().Contain(source =>
            source.Id == "ffg-thunders-edge");
    }

    [Fact]
    public async Task GeneratedFactionCatalogContainsCreussBreakthrough()
    {
        var path = DataPath("faction-rulings.json");
        await using var stream = File.OpenRead(path);
        var catalog = await JsonSerializer.DeserializeAsync<RulingsCatalog>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        catalog.Should().NotBeNull();
        catalog!.Items.Should().HaveCount(30);
        catalog.Items
            .Single(item => item.Title == "The Ghosts of Creuss")
            .Sections.Should().Contain(section =>
                section.Title == "Particle Synthesis (Breakthrough)");
    }

    [Fact]
    public async Task GeneratedComponentCatalogProvidesCurrentCardAndTechnologyNotes()
    {
        var path = DataPath("component-rulings.json");
        await using var stream = File.OpenRead(path);
        var catalog = await JsonSerializer.DeserializeAsync<RulingsCatalog>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var actionNotes = RulingsCatalogIndex.ForItem(catalog, "action-cards");
        var technologyNotes = RulingsCatalogIndex.ForItem(catalog, "technology");

        actionNotes.Should().ContainKey(RulingsCatalogIndex.Normalize("Coup D'etat"));
        technologyNotes.Should().ContainKey(
            RulingsCatalogIndex.Normalize("X-89 Bacterial Weapon Ω"));
    }

    private static string DataPath(string fileName) =>
        Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../src/TwilightImperiumUltimate.Web/wwwroot/data",
                fileName));
}