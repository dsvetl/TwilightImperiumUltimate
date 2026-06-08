using FluentAssertions;
using TwilightImperiumUltimate.Contracts.Enums;
using TwilightImperiumUltimate.Web.Components.Library;
using Xunit;

namespace TwilightImperiumUltimate.Tests.Web;

public class ContentSourceFiltersTests
{
    private static readonly GameVersion[] OfficialVersions =
    [
        GameVersion.BaseGame,
        GameVersion.ProphecyOfKings,
        GameVersion.CodexRecolo,
    ];

    [Fact]
    public void VersionItemsHidesSecondaryRowWhenOneVersionIsAvailable()
    {
        var items = ContentSourceFilters.VersionItems(
            OfficialVersions,
            "official",
            [GameVersion.ProphecyOfKings]);

        items.Should().BeEmpty();
    }

    [Fact]
    public void VersionItemsHidesSecondaryRowWhenNoVersionsAreAvailable()
    {
        var items = ContentSourceFilters.VersionItems(
            OfficialVersions,
            "official",
            []);

        items.Should().BeEmpty();
    }

    [Fact]
    public void VersionItemsKeepsStableVersionsAndDisablesEmptyOnes()
    {
        var items = ContentSourceFilters.VersionItems(
            OfficialVersions,
            "official",
            [GameVersion.BaseGame, GameVersion.CodexRecolo]);

        items.Select(item => item.Value).Should().Equal(
            string.Empty,
            GameVersion.BaseGame.ToString(),
            GameVersion.ProphecyOfKings.ToString(),
            GameVersion.CodexRecolo.ToString());
        items.Single(item => item.Value == GameVersion.ProphecyOfKings.ToString())
            .Disabled.Should().BeTrue();
    }

    [Fact]
    public void NormalizeVersionClearsSelectionWhenSecondaryChoiceIsNotMeaningful()
    {
        var normalized = ContentSourceFilters.NormalizeVersion(
            GameVersion.ProphecyOfKings.ToString(),
            OfficialVersions,
            "official",
            [GameVersion.ProphecyOfKings]);

        normalized.Should().BeEmpty();
    }

    [Fact]
    public void NormalizeVersionRejectsVersionDisabledByCurrentContentFilter()
    {
        var normalized = ContentSourceFilters.NormalizeVersion(
            GameVersion.ProphecyOfKings.ToString(),
            OfficialVersions,
            "official",
            [GameVersion.BaseGame, GameVersion.CodexRecolo]);

        normalized.Should().BeEmpty();
    }
}
