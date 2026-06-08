using System.Net.Http.Json;

namespace TwilightImperiumUltimate.Web.Components.Game.Technologies;

public partial class TechnologyBrowser
{
    private static readonly IReadOnlyList<FilterChipItem> TypeFilters =
    [
        new("biotic", "Biotic"),
        new("cybernetic", "Cybernetic"),
        new("propulsion", "Propulsion"),
        new("warfare", "Warfare"),
        new("unitupgrade", "Unit Upgrades"),
        new("faction", "Faction"),
    ];

    private IReadOnlyList<TechnologyDto> _technologies = [];
    private IReadOnlyDictionary<string, IReadOnlyList<FaqDto>> _faqs =
        new Dictionary<string, IReadOnlyList<FaqDto>>(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyDictionary<string, string> _importedNotes =
        new Dictionary<string, string>(StringComparer.Ordinal);
    private string _type = "biotic";
    private string _source = "all";
    private string _version = string.Empty;
    private bool _loading = true;
    private bool _failed;

    [Parameter]
    public string? Type { get; set; }

    [Parameter]
    public string? Source { get; set; }

    [Parameter]
    public string? Version { get; set; }

    [Inject]
    private ILibraryDataService Data { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private HttpClient Http { get; set; } = default!;

    private IEnumerable<TechnologyDto> VisibleTechnologies => _technologies
        .Where(MatchesType)
        .Where(item => ContentSourceFilters.Matches(item.GameVersion, _source, _version))
        .OrderBy(item => LibraryFormatting.VersionOrder(item.GameVersion))
        .ThenBy(item => item.IsFactionTechnology)
        .ThenBy(item => item.Name);
    private IReadOnlyList<FilterChipItem> VersionFilters =>
        ContentSourceFilters.VersionItems(
            _technologies.Select(item => item.GameVersion),
            _source,
            _technologies.Where(MatchesType).Select(item => item.GameVersion));

    protected override async Task OnInitializedAsync() => await Load();

    protected override void OnParametersSet()
    {
        var requested = Type?.ToLowerInvariant() ?? "biotic";
        _type = TypeFilters.Any(item => item.Value == requested) ? requested : "biotic";
        NormalizeSourceFilters();
    }

    private async Task Load(bool refresh = false)
    {
        _loading = true;
        _failed = false;
        try
        {
            var technologiesTask = Data.GetTechnologiesAsync(refresh);
            var rulesTask = Data.GetRulesAsync(refresh);
            var rulingsTask = LoadImportedRulingsAsync();
            await Task.WhenAll(technologiesTask, rulesTask, rulingsTask);
            _technologies = await technologiesTask;
            _importedNotes = RulingsCatalogIndex.ForItem(await rulingsTask, "technology");
            _faqs = (await rulesTask).Faqs
                .GroupBy(faq => faq.ComponentName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<FaqDto>)group.ToArray(),
                    StringComparer.OrdinalIgnoreCase);
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

    private bool MatchesType(TechnologyDto item) => _type switch
    {
        "faction" => item.IsFactionTechnology,
        "unitupgrade" => item.Type == TechnologyType.UnitUpgrade,
        _ => item.Type.ToString().Equals(_type, StringComparison.OrdinalIgnoreCase),
    };

    private static string TechnologyKey(TechnologyDto technology) =>
        $"{technology.TechnologyName}:{technology.GameVersion}:{technology.FactionName}";
    private IReadOnlyList<FaqDto> FaqsFor(TechnologyDto technology) =>
        _faqs.GetValueOrDefault(technology.TechnologyName.ToString(), []);

    private string NotesFor(TechnologyDto technology)
    {
        var titleKey = RulingsCatalogIndex.Normalize(
            LibraryFormatting.Identifier(technology.Name));
        if (_importedNotes.TryGetValue(titleKey, out var notes))
            return notes;

        return RulesContentCatalog.GetComponentNotes()
            .GetValueOrDefault(technology.TechnologyName.ToString(), string.Empty);
    }

    private async Task<RulingsCatalog?> LoadImportedRulingsAsync()
    {
        try
        {
            return await Http.GetFromJsonAsync<RulingsCatalog>(
                Navigation.ToAbsoluteUri("/data/component-rulings.json"));
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    private void SetType(string value)
    {
        _type = value;
        NormalizeSelectedVersion();

        Navigation.NavigateTo(
            Navigation.GetUriWithQueryParameters(new Dictionary<string, object?>
            {
                ["type"] = value == "biotic" ? null : value,
                ["source"] = _source == "all" ? null : _source,
                ["version"] = string.IsNullOrWhiteSpace(_version) ? null : _version,
            }),
            replace: true);
    }

    private void SetSource(string value)
    {
        _source = value;
        _version = string.Empty;
        UpdateSourceUrl();
    }

    private void SetVersion(string value)
    {
        _version = value;
        UpdateSourceUrl();
    }

    private void UpdateSourceUrl()
    {
        Navigation.NavigateTo(
            Navigation.GetUriWithQueryParameters(new Dictionary<string, object?>
            {
                ["type"] = _type == "biotic" ? null : _type,
                ["source"] = _source == "all" ? null : _source,
                ["version"] = string.IsNullOrWhiteSpace(_version) ? null : _version,
            }),
            replace: true);
    }

    private void NormalizeSourceFilters()
    {
        _source = ContentSourceFilters.NormalizeSource(Source ?? _source);
        _version = ContentSourceFilters.NormalizeVersion(
            Version ?? _version,
            _technologies.Select(item => item.GameVersion),
            _source,
            _technologies.Where(MatchesType).Select(item => item.GameVersion));
    }

    private void NormalizeSelectedVersion() =>
        _version = ContentSourceFilters.NormalizeVersion(
            _version,
            _technologies.Select(item => item.GameVersion),
            _source,
            _technologies.Where(MatchesType).Select(item => item.GameVersion));

    private Task Retry() => Load(refresh: true);
}
