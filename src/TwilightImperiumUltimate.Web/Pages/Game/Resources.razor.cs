namespace TwilightImperiumUltimate.Web.Pages.Game;

public partial class Resources
{
    [SupplyParameterFromQuery(Name = "type")]
    public string? Type { get; set; }
}
