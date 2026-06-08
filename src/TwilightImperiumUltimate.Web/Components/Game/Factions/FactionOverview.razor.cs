using Microsoft.JSInterop;

namespace TwilightImperiumUltimate.Web.Components.Game.Factions;

public partial class FactionOverview : IAsyncDisposable
{
    private readonly string _previewIdPrefix = $"faction-sheet-{Guid.NewGuid():N}";
    private ElementReference _root;
    private IJSObjectReference? _module;

    [Parameter, EditorRequired]
    public FactionDto Faction { get; set; } = default!;

    [Inject]
    private IPathProvider PathProvider { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private IReadOnlyList<FactionSheet> FactionSheets =>
    [
        new(
            "Front",
            FactionSheetUrl(front: true),
            $"{_previewIdPrefix}-front"),
        new(
            "Back",
            FactionSheetUrl(front: false),
            $"{_previewIdPrefix}-back"),
    ];

    private string SystemInfo
    {
        get
        {
            var resource = Faction.FactionName.GetFactionUIText(FactionResourceType.SystemInfo);
            return string.IsNullOrWhiteSpace(resource) ? Faction.SystemInfo : resource;
        }
    }

    private IReadOnlyList<SystemStat> SystemStats
    {
        get
        {
            var resource = Faction.FactionName.GetFactionUIText(FactionResourceType.SystemStats);
            var stats = string.IsNullOrWhiteSpace(resource) ? Faction.SystemStats : resource;

            return stats
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ParseStat)
                .ToArray();
        }
    }

    private static SystemStat ParseStat(string stat)
    {
        var separator = stat.IndexOf('.');
        return separator < 0
            ? new(stat, string.Empty)
            : new(stat[..separator], stat[(separator + 1)..]);
    }

    private string FactionSheetUrl(bool front) =>
        PathProvider.GetFactionSheetPath(Faction.FactionName.ToString(), front)
            .Replace('\\', '/');

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        try
        {
            _module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./Components/Game/Factions/FactionOverview.razor.js");
            await _module.InvokeVoidAsync("initializeFactionSheetPreviews", _root);
        }
        catch (JSException)
        {
            // The full-size image links remain available if enhancement fails.
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
            return;

        try
        {
            await _module.InvokeVoidAsync("disposeFactionSheetPreviews", _root);
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The browser connection is already gone.
        }
    }

    private sealed record FactionSheet(string Label, string ImageUrl, string PreviewId);

    private sealed record SystemStat(string Label, string Value);
}
