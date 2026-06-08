namespace TwilightImperiumUltimate.Web.Components.Game.Factions;

public partial class FactionBrowser
{
    private IReadOnlyList<FactionDto> _factions = [];
    private string _source = "all";
    private string _version = string.Empty;
    private bool _loading = true;
    private bool _failed;

    [Parameter]
    public string? Source { get; set; }

    [Parameter]
    public string? Version { get; set; }

    [Parameter]
    public string? Faction { get; set; }

    [Parameter]
    public string? Info { get; set; }

    [Inject]
    private ILibraryDataService Data { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private IReadOnlyList<FactionDto> VisibleFactions => _factions
        .Where(faction => ContentSourceFilters.Matches(faction.GameVersion, _source, _version))
        .OrderBy(faction => LibraryFormatting.VersionOrder(faction.GameVersion))
        .ThenBy(faction => LibraryFormatting.Faction(faction.FactionName))
        .ToArray();
    private IReadOnlyList<FilterChipItem> VersionFilters =>
        ContentSourceFilters.VersionItems(_factions.Select(faction => faction.GameVersion), _source);

    private FactionDto? SelectedFaction => string.IsNullOrWhiteSpace(Faction)
        ? null
        : _factions.FirstOrDefault(item =>
            item.FactionName.ToString().Equals(Faction, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync() => await Load();

    protected override void OnParametersSet()
    {
        NormalizeSourceFilters();
    }

    private async Task Load(bool refresh = false)
    {
        _loading = true;
        _failed = false;
        try
        {
            _factions = await Data.GetFactionsAsync(refresh);
            NormalizeSourceFilters();
        }
        catch
        {
            _failed = true;
        }
        finally
        {
            _loading = false;
        }
    }

    private Task Retry() => Load(refresh: true);

    private void SetSource(string value)
    {
        _source = value;
        _version = string.Empty;
        UpdateUrl(Faction);
    }

    private void SetVersion(string value)
    {
        _version = value;
        UpdateUrl(Faction);
    }

    private void Select(FactionDto faction) => UpdateUrl(faction.FactionName.ToString());

    private void ClearSelection() => UpdateUrl(null);
    private void SetInfo(string info) => UpdateUrl(Faction, info);

    private void UpdateUrl(string? faction, string? info = null)
    {
        var uri = Navigation.GetUriWithQueryParameters(
            Navigation.ToAbsoluteUri("/game/factions").ToString(),
            new Dictionary<string, object?>
            {
                ["source"] = _source == "all" ? null : _source,
                ["version"] = string.IsNullOrWhiteSpace(_version) ? null : _version,
                ["faction"] = faction,
                ["info"] = string.IsNullOrWhiteSpace(info) || info == "overview" ? null : info,
            });
        Navigation.NavigateTo(uri, replace: true);
    }

    private void NormalizeSourceFilters()
    {
        _source = ContentSourceFilters.NormalizeSource(Source ?? _source);
        _version = ContentSourceFilters.NormalizeVersion(
            Version ?? _version,
            _factions.Select(faction => faction.GameVersion),
            _source);
    }
}
