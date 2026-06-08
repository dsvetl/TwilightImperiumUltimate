namespace TwilightImperiumUltimate.Web.Pages.Game;

public partial class Technologies
{
    [SupplyParameterFromQuery(Name = "type")]
    public string? Type { get; set; }

    [SupplyParameterFromQuery(Name = "source")]
    public string? Source { get; set; }

    [SupplyParameterFromQuery(Name = "version")]
    public string? Version { get; set; }
}
