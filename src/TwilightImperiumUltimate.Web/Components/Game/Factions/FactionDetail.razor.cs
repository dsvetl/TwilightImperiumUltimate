namespace TwilightImperiumUltimate.Web.Components.Game.Factions;

public partial class FactionDetail
{
    private static readonly IReadOnlyList<FactionSection> Sections =
    [
        new("overview", "Overview"),
        new("abilities", "Abilities"),
        new("setup", "Setup"),
        new("components", "Faction Components"),
        new("leaders", "Leaders"),
        new("lore", "Lore"),
        new("faq", "FAQ"),
    ];

    [Parameter, EditorRequired]
    public FactionDto Faction { get; set; } = default!;

    [Parameter]
    public string? ActiveSection { get; set; }

    [Parameter]
    public EventCallback<string> OnSectionChanged { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Inject]
    private IPathProvider PathProvider { get; set; } = default!;

    private string CurrentSection => Sections.Any(section =>
        section.Key.Equals(ActiveSection, StringComparison.OrdinalIgnoreCase))
            ? ActiveSection!.ToLowerInvariant()
            : "overview";

    private string Quote
    {
        get
        {
            var resourceQuote = Faction.FactionName.GetFactionUIText(FactionResourceType.Quote);
            return string.IsNullOrWhiteSpace(resourceQuote) ? Faction.Quote : resourceQuote;
        }
    }

    private Task SelectSection(string section) => OnSectionChanged.InvokeAsync(section);

    private sealed record FactionSection(string Key, string Label);
}
