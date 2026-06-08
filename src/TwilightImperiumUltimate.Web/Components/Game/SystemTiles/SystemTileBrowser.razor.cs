namespace TwilightImperiumUltimate.Web.Components.Game.SystemTiles;

public partial class SystemTileBrowser
{
    private static readonly IReadOnlyList<FilterChipItem> LensFilters =
    [
        new("systems", "Systems"),
        new("planets", "Planets"),
    ];

    private static readonly IReadOnlyList<FilterChipItem> KindFilters =
    [
        new("all", "All"),
        new("home", "Home"),
        new("regular", "Regular"),
        new("anomaly", "Anomalies"),
        new("hyperlane", "Hyperlanes"),
    ];

    private static readonly IReadOnlyList<FilterChipItem> TraitFilters =
    [
        new("all", "All"),
        new("cultural", "Cultural"),
        new("hazardous", "Hazardous"),
        new("industrial", "Industrial"),
        new("legendary", "Legendary"),
    ];

    private IReadOnlyList<SystemTileDto> _tiles = [];
    private string _view = "systems";
    private string _kind = "all";
    private string _trait = "all";
    private string _source = "all";
    private string _version = string.Empty;
    private bool _loading = true;
    private bool _failed;

    [Parameter]
    public string? View { get; set; }

    [Parameter]
    public string? Kind { get; set; }

    [Parameter]
    public string? Trait { get; set; }

    [Parameter]
    public string? LegacyType { get; set; }

    [Parameter]
    public string? Tile { get; set; }

    [Parameter]
    public string? Planet { get; set; }

    [Parameter]
    public string? Source { get; set; }

    [Parameter]
    public string? Version { get; set; }

    [Inject]
    private ILibraryDataService Data { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private IReadOnlyList<SystemTileDto> VisibleTiles => _tiles
        .Where(tile => !string.IsNullOrWhiteSpace(tile.SystemTileCode))
        .Where(MatchesKind)
        .Where(MatchesSource)
        .OrderBy(tile => int.TryParse(tile.SystemTileCode, out var number) ? number : int.MaxValue)
        .ThenBy(tile => tile.SystemTileCode)
        .ToArray();

    private IReadOnlyList<BoardPlanet> VisiblePlanets => _tiles
        .Where(MatchesSource)
        .SelectMany(tile => tile.Planets.Select(planet => new BoardPlanet(planet, tile)))
        .Where(item => MatchesTrait(item.Planet))
        .OrderBy(item => LibraryFormatting.VersionOrder(item.Planet.GameVersion))
        .ThenBy(item => PlanetName(item.Planet))
        .ToArray();

    private IReadOnlyList<FilterChipItem> VersionFilters =>
        ContentSourceFilters.VersionItems(
            _tiles.Select(tile => tile.GameVersion),
            _source,
            VersionsForCurrentContext());

    private PlanetDto? SelectedPlanet => string.IsNullOrWhiteSpace(Planet)
        ? null
        : _tiles.SelectMany(tile => tile.Planets)
            .FirstOrDefault(item => item.PlanetName.ToString()
                .Equals(Planet, StringComparison.OrdinalIgnoreCase));

    private SystemTileDto? SelectedTile => !string.IsNullOrWhiteSpace(Tile)
        ? _tiles.FirstOrDefault(item => item.SystemTileCode.Equals(Tile, StringComparison.OrdinalIgnoreCase))
        : SelectedPlanet is null
            ? null
            : _tiles.FirstOrDefault(item => item.Planets.Any(planet => planet.Id == SelectedPlanet.Id));

    protected override async Task OnInitializedAsync() => await Load();

    protected override void OnParametersSet()
    {
        _view = View?.Equals("planets", StringComparison.OrdinalIgnoreCase) == true
            ? "planets"
            : "systems";
        _kind = Normalize(Kind ?? (_view == "systems" ? LegacyType : null), KindFilters);
        _trait = Normalize(Trait ?? (_view == "planets" ? LegacyType : null), TraitFilters);
        NormalizeSourceFilters(Source, Version);
    }

    private async Task Load(bool refresh = false)
    {
        _loading = true;
        _failed = false;
        try
        {
            _tiles = await Data.GetSystemTilesAsync(refresh);
            NormalizeSourceFilters(_source, _version);
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

    private bool MatchesSource(SystemTileDto tile) =>
        ContentSourceFilters.Matches(tile.GameVersion, _source, _version);

    private bool MatchesKind(SystemTileDto tile) => _kind switch
    {
        "all" => true,
        "home" => tile.SystemTileCategory == SystemTileCategory.Green,
        "regular" => tile.SystemTileCategory is SystemTileCategory.Blue or SystemTileCategory.Red
            && tile.AnomalyName == AnomalyName.None,
        "anomaly" => tile.AnomalyName != AnomalyName.None,
        "hyperlane" => tile.SystemTileCategory == SystemTileCategory.Hyperlane,
        _ => true,
    };

    private bool MatchesTrait(PlanetDto planet) => _trait switch
    {
        "all" => true,
        "legendary" => planet.IsLegendary || planet.PlanetTrait == PlanetTrait.Legendary,
        _ => planet.PlanetTrait.ToString().Contains(_trait, StringComparison.OrdinalIgnoreCase),
    };

    private static string Category(SystemTileDto tile) => tile.SystemTileCategory switch
    {
        SystemTileCategory.Green => "Home system",
        SystemTileCategory.Hyperlane => "Hyperlane",
        SystemTileCategory.MecatolRex => "Mecatol Rex",
        _ when tile.AnomalyName != AnomalyName.None =>
            LibraryFormatting.Identifier(tile.AnomalyName.ToString()),
        _ => "System",
    };

    private bool ShowCategory(SystemTileDto tile) =>
        _kind is "all" or "anomaly" || Category(tile) == "Mecatol Rex";

    private static string PlanetName(PlanetDto planet) =>
        LibraryFormatting.Identifier(planet.PlanetName.ToString());

    private static string Normalize(string? requested, IReadOnlyList<FilterChipItem> filters)
    {
        var normalized = requested?.ToLowerInvariant() ?? "all";
        return filters.Any(item => item.Value == normalized) ? normalized : "all";
    }

    private void SetView(string value)
    {
        _view = value;
        ResetUnavailableVersion();
        UpdateUrl(replace: true);
    }

    private void SetContextFilter(string value)
    {
        if (_view == "planets")
            _trait = value;
        else
            _kind = value;

        ResetUnavailableVersion();
        UpdateUrl(replace: true);
    }

    private void SetSource(string value)
    {
        _source = value;
        _version = string.Empty;
        UpdateUrl(replace: true);
    }

    private void SetVersion(string value)
    {
        _version = value;
        UpdateUrl(replace: true);
    }

    private void SelectTile(SystemTileDto tile) =>
        UpdateUrl(tile: tile.SystemTileCode);

    private void SelectPlanet(PlanetDto planet) =>
        UpdateUrl(planet: planet.PlanetName.ToString());

    private void ShowSelectedSystem() =>
        UpdateUrl(tile: SelectedTile?.SystemTileCode);

    private void ClearSelection() => UpdateUrl();

    private Task Retry() => Load(refresh: true);

    private void UpdateUrl(
        string? tile = null,
        string? planet = null,
        bool replace = false)
    {
        var uri = Navigation.GetUriWithQueryParameters(
            Navigation.ToAbsoluteUri("/game/board").ToString(),
            new Dictionary<string, object?>
            {
                ["view"] = _view == "planets" ? "planets" : null,
                ["kind"] = _view == "systems" && _kind != "all" ? _kind : null,
                ["trait"] = _view == "planets" && _trait != "all" ? _trait : null,
                ["source"] = _source == "all" ? null : _source,
                ["version"] = string.IsNullOrWhiteSpace(_version) ? null : _version,
                ["tile"] = tile,
                ["planet"] = planet,
            });
        Navigation.NavigateTo(uri, forceLoad: false, replace: replace);
    }

    private void NormalizeSourceFilters(string? source, string? version)
    {
        _source = ContentSourceFilters.NormalizeSource(source);
        _version = ContentSourceFilters.NormalizeVersion(
            version,
            _tiles.Select(tile => tile.GameVersion),
            _source,
            VersionsForCurrentContext());
    }

    private void ResetUnavailableVersion() =>
        _version = ContentSourceFilters.NormalizeVersion(
            _version,
            _tiles.Select(tile => tile.GameVersion),
            _source,
            VersionsForCurrentContext());

    private IEnumerable<GameVersion> VersionsForCurrentContext() =>
        _view == "planets"
            ? _tiles.SelectMany(tile => tile.Planets)
                .Where(MatchesTrait)
                .Select(planet => planet.GameVersion)
            : _tiles.Where(MatchesKind).Select(tile => tile.GameVersion);

    private sealed record BoardPlanet(PlanetDto Planet, SystemTileDto Tile);
}
