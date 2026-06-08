using Routes = TwilightImperiumUltimate.Web.Pages.Pages;

namespace TwilightImperiumUltimate.Web.Components.Shared.Navigation;

public static class NavigationCatalog
{
    private static readonly IReadOnlyList<NavigationItem> GameItems =
    [
        new("factions", "Factions", Routes.Factions),
        new("technologies", "Technologies", Routes.Technologies),
        new("cards", "Cards", Routes.Cards),
        new("board", "Board", Routes.Board),
        new("reference", "Reference", Routes.GameReference),
        new("resources", "Resources", Routes.GameResources),
    ];

    public static IReadOnlyList<NavigationItem> Main { get; } =
    [
        new("news", Strings.Page_News, Routes.Index),
        new("game", Strings.Page_Game, Routes.Game, GameItems),
        new("community", Strings.Page_Community, Routes.Community,
        [
            new("galaxy-map", Strings.Page_GalaxyMap, Routes.GalaxyMap),
            new("maps", Strings.Page_MapArchive, Routes.MapsArchive),
            new("slices", Strings.Page_SlicesArchive, Routes.SlicesArchive),
            new("discord", Strings.Page_Discord, Routes.Discord),
            new("async", Strings.Page_Async, Routes.Async),
            new("websites", Strings.Page_OtherWebsites, Routes.Websites),
        ]),
        new("tigl", Strings.Page_Tigl, Routes.Tigl,
        [
            new("info", Strings.Page_TiglInfo, Routes.Tigl),
            new("register", Strings.Page_TiglRegister, Routes.TiglRegister),
            new("report", Strings.Page_TiglReportGame, Routes.TiglReportGame),
            new("leaderboard", Strings.TiglMenu_Leaderboard, Routes.TiglLeaderboard),
            new("statistics", Strings.TiglMenu_Statistics, Routes.TiglStatistics),
            new("players", Strings.TiglMenu_Players, Routes.TiglPlayers),
            new("games", Strings.TiglMenu_GameReports, Routes.TiglGames),
            new("rankings", Strings.RankingsMenu_Ranks, Routes.TiglRankings),
            new("leaders", Strings.RankingsMenu_Leaders, Routes.TiglLeaders),
            new("achievements", Strings.RankingsMenu_Achievements, Routes.TiglAchievements),
        ]),
        new("tools", Strings.Page_Tools, Routes.Tools,
        [
            new("tracker", Strings.Page_GameTracker, Routes.GameTracker, OpenInNewTab: true),
            new("colors", Strings.Page_ColorPicker, Routes.ColorPicker),
            new("faction-draft", Strings.Page_FactionDraft, Routes.FactionDraft),
            new("milty", Strings.Page_MiltyDraft, Routes.MiltyDraft),
            new("slices", Strings.Page_SliceGenerator, Routes.SliceGenerator),
            new("maps", Strings.Page_MapGenerator, Routes.MapGenerator),
            new("cards", Strings.Page_CardGenerator, Routes.CardGenerator),
        ]),
    ];

    public static IReadOnlyList<NavigationItem> Game => GameItems;
}
