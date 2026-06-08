namespace TwilightImperiumUltimate.Web.Pages.Game;

public partial class Factions
{
    [SupplyParameterFromQuery(Name = "source")]
    public string? Source { get; set; }

    [SupplyParameterFromQuery(Name = "version")]
    public string? Version { get; set; }

    [SupplyParameterFromQuery(Name = "faction")]
    public string? Faction { get; set; }

    [SupplyParameterFromQuery(Name = "info")]
    public string? Info { get; set; }
}
