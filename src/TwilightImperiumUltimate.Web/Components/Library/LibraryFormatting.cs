using System.Text.RegularExpressions;
using TwilightImperiumUltimate.Web.Helpers.Enums;

namespace TwilightImperiumUltimate.Web.Components.Library;

public static partial class LibraryFormatting
{
    public static string Identifier(string value) =>
        IdentifierBoundary().Replace(value, " $1").Trim();

    public static string Version(GameVersion version)
    {
        var displayName = version.GetDisplayName();
        return displayName.Contains('.') ? Identifier(version.ToString()) : displayName;
    }

    public static string Faction(FactionName faction) =>
        faction.GetFactionUIText(FactionResourceType.Title);

    public static bool IsCommunity(GameVersion version) => version is
        GameVersion.DiscordantStars or
        GameVersion.UnchartedSpace or
        GameVersion.AscendantSun or
        GameVersion.TtsHyperlines;

    public static string Source(GameVersion version) => IsCommunity(version) ? "Community" : "Official";

    public static string SourceValue(GameVersion version) => IsCommunity(version) ? "community" : "official";

    public static int VersionOrder(GameVersion version) => version switch
    {
        GameVersion.BaseGame => 0,
        GameVersion.ProphecyOfKings => 10,
        GameVersion.CodexRecolo => 20,
        GameVersion.CodexOrdinian => 21,
        GameVersion.CodexAffinity => 22,
        GameVersion.CodexVigil => 23,
        GameVersion.CodexLiberation => 24,
        GameVersion.ThundersEdge => 30,
        _ when IsCommunity(version) => 100,
        _ => 200,
    };

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex IdentifierBoundary();
}