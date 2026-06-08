namespace TwilightImperiumUltimate.Web.Pages.Game;

public partial class FactionsDS
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "faction")]
    public string Faction { get; set; } = string.Empty;

    [Parameter]
    [SupplyParameterFromQuery(Name = "info")]
    public string Info { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        var query = new List<string> { "source=community" };

        if (!string.IsNullOrWhiteSpace(Faction))
            query.Add($"faction={Uri.EscapeDataString(Faction)}");

        if (!string.IsNullOrWhiteSpace(Info))
            query.Add($"info={Uri.EscapeDataString(Info)}");

        NavigationManager.NavigateTo($"/game/factions?{string.Join("&", query)}", replace: true);
    }
}
