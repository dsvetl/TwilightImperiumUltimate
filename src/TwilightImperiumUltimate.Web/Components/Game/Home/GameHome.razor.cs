namespace TwilightImperiumUltimate.Web.Components.Game.Home;

public partial class GameHome
{
    private static readonly IReadOnlyList<PortalItem> Items =
    [
        new(
            "Civilizations",
            "Factions",
            "Abilities, setup, lore, and faction identity.",
            "/game/factions",
            "/resources/images/shared/factionicons/TheArborec.webp"),
        new(
            "Research",
            "Technologies",
            "Core research paths and faction advances.",
            "/game/technologies",
            "/resources/images/shared/technologies/Propulsion.webp"),
        new(
            "Deck archive",
            "Cards",
            "Action cards, agendas, objectives, relics, and more.",
            "/game/cards",
            "/resources/images/shared/cardicons/ActionCard.webp"),
        new(
            "Galaxy atlas",
            "Board",
            "System tiles, planets, anomalies, wormholes, and home worlds.",
            "/game/board",
            "/resources/images/shared/tiles/large/Tile18.webp"),
        new(
            "Rules library",
            "Reference",
            "Browse the complete rules reference by topic.",
            "/game/reference",
            "/resources/images/en-US/cards/agenda/NewConstitution.webp"),
        new(
            "Downloads",
            "Resources",
            "Official rulebooks, references, and supporting documents.",
            "/game/resources",
            "/resources/images/shared/icons/pdfdownload.webp"),
    ];

    private sealed record PortalItem(
        string Eyebrow,
        string Title,
        string Description,
        string Url,
        string Image);
}
