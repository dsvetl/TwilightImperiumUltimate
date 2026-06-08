using System.Net.Http.Json;

namespace TwilightImperiumUltimate.Web.Components.Game.Cards;

public partial class CardBrowser
{
    private static readonly IReadOnlyList<FilterChipItem> CardTypes =
    [
        new("action", "Action"),
        new("agenda", "Agenda"),
        new("objective", "Objectives"),
        new("exploration", "Exploration"),
        new("relic", "Relics"),
        new("frontier", "Frontier"),
        new("promissory", "Promissory"),
        new("strategy", "Strategy"),
    ];

    private IReadOnlyList<LibraryCardItem> _cards = [];
    private IReadOnlyDictionary<string, IReadOnlyList<FaqDto>> _faqs =
        new Dictionary<string, IReadOnlyList<FaqDto>>(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyDictionary<string, string> _importedNotes =
        new Dictionary<string, string>(StringComparer.Ordinal);

    private string _type = "action";
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

    private IReadOnlyList<LibraryCardItem> VisibleCards => _cards
        .Where(card => ContentSourceFilters.Matches(card.Version, _source, _version))
        .OrderBy(card => LibraryFormatting.VersionOrder(card.Version))
        .ThenBy(card => card.Name)
        .ToArray();

    private IReadOnlyList<FilterChipItem> VersionFilters
    {
        get
        {
            if (_source == "all")
                return [];

            var availableVersions = _cards
                .Where(card => LibraryFormatting.SourceValue(card.Version) == _source)
                .Select(card => card.Version)
                .ToHashSet();

            return
            [
                new(string.Empty, "All"),
                .. Enum.GetValues<GameVersion>()
                    .Where(version => version is not GameVersion.Deprecated and not GameVersion.Custom)
                    .Where(version => LibraryFormatting.SourceValue(version) == _source)
                    .OrderBy(LibraryFormatting.VersionOrder)
                    .Select(version => new FilterChipItem(
                        version.ToString(),
                        LibraryFormatting.Version(version),
                        Disabled: !availableVersions.Contains(version))),
            ];
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        var normalized = LibraryDataService.NormalizeCardType(Type);
        if (!_loading && normalized == _type)
        {
            var versionWasReset = NormalizeSourceFilters();
            if (versionWasReset)
                UpdateSourceUrl();

            return;
        }

        _type = normalized;
        await Load();
    }

    private async Task Load(bool refresh = false)
    {
        _loading = true;
        _failed = false;
        try
        {
            var cardsTask = Data.GetCardsAsync(_type, refresh);
            var rulesTask = Data.GetRulesAsync(refresh);
            var rulingsTask = LoadImportedRulingsAsync();
            await Task.WhenAll(cardsTask, rulesTask, rulingsTask);
            _cards = await cardsTask;
            _importedNotes = RulingsCatalogIndex.ForItem(await rulingsTask, RulingsKey(_type));
            _faqs = (await rulesTask).Faqs
                .GroupBy(faq => faq.ComponentName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<FaqDto>)group.ToArray(),
                    StringComparer.OrdinalIgnoreCase);
            var versionWasReset = NormalizeSourceFilters();
            if (versionWasReset)
                UpdateSourceUrl();
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

    private void SetType(string value)
    {
        _source = "all";
        _version = string.Empty;

        var uri = Navigation.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["type"] = value == "action" ? null : value,
            ["source"] = null,
            ["version"] = null,
        });
        Navigation.NavigateTo(uri, replace: true);
    }

    private void SetSource(string value)
    {
        _source = value;
        _version = string.Empty;
        UpdateSourceUrl();
    }

    private void SetVersion(string value)
    {
        if (VersionFilters.Any(item => item.Value == value && item.Disabled))
            return;

        _version = value;
        UpdateSourceUrl();
    }

    private void UpdateSourceUrl()
    {
        Navigation.NavigateTo(
            Navigation.GetUriWithQueryParameters(new Dictionary<string, object?>
            {
                ["type"] = _type == "action" ? null : _type,
                ["source"] = _source == "all" ? null : _source,
                ["version"] = string.IsNullOrWhiteSpace(_version) ? null : _version,
            }),
            replace: true);
    }

    private bool NormalizeSourceFilters()
    {
        _source = ContentSourceFilters.NormalizeSource(Source ?? _source);
        var requestedVersion = Version ?? _version;
        var normalizedVersion = ContentSourceFilters.NormalizeVersion(
            requestedVersion,
            _cards.Select(card => card.Version),
            _source);
        _version = normalizedVersion;

        return !string.IsNullOrWhiteSpace(requestedVersion)
            && string.IsNullOrWhiteSpace(normalizedVersion);
    }

    private IReadOnlyList<FaqDto> FaqsFor(LibraryCardItem card) =>
        _faqs.GetValueOrDefault(card.AssetKey, []);

    private string NotesFor(LibraryCardItem card)
    {
        var titleKey = RulingsCatalogIndex.Normalize(LibraryFormatting.Identifier(card.Name));
        if (_importedNotes.TryGetValue(titleKey, out var notes))
            return notes;

        return RulesContentCatalog.GetComponentNotes()
            .GetValueOrDefault(card.AssetKey, string.Empty);
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

    private static string RulingsKey(string type) => type switch
    {
        "action" => "action-cards",
        "agenda" => "agendas",
        "objective" => "objectives",
        "exploration" or "frontier" => "exploration-cards",
        "relic" => "relics",
        "promissory" => "promissory-notes",
        _ => string.Empty,
    };

    private Task Retry() => Load(refresh: true);
}
