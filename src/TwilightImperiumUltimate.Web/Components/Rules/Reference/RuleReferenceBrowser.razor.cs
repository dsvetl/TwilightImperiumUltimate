using System.Net.Http.Json;

namespace TwilightImperiumUltimate.Web.Components.Rules.Reference;

public sealed partial class RuleReferenceBrowser
{
    private IReadOnlyList<RulesReferenceEntry> _rules = [];
    private IReadOnlyDictionary<string, RulesReferenceSource> _sources =
        new Dictionary<string, RulesReferenceSource>(StringComparer.OrdinalIgnoreCase);
    private bool _loading = true;
    private bool _failed;

    [Parameter] public string? RuleKey { get; set; }
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IReadOnlyList<RulesReferenceEntry> VisibleRules => _rules.OrderBy(rule => rule.Title).ToArray();
    private IReadOnlyList<IGrouping<string, RulesReferenceEntry>> RuleGroups => VisibleRules
        .GroupBy(rule => Initial(rule.Title))
        .ToArray();
    private IReadOnlySet<string> AvailableLetters => RuleGroups
        .Select(group => group.Key)
        .ToHashSet(StringComparer.Ordinal);
    private RulesReferenceEntry? SelectedRule => string.IsNullOrWhiteSpace(RuleKey)
        ? null
        : _rules.FirstOrDefault(rule =>
            rule.Key.Equals(RuleKey, StringComparison.OrdinalIgnoreCase)
            || rule.LegacyId?.ToString() == RuleKey);
    private int SelectedIndex => SelectedRule is null
        ? -1
        : Array.FindIndex(_rules.ToArray(), rule => rule.Key == SelectedRule.Key);
    private RulesReferenceEntry? PreviousRule => SelectedIndex > 0 ? _rules[SelectedIndex - 1] : null;
    private RulesReferenceEntry? NextRule => SelectedIndex >= 0 && SelectedIndex < _rules.Count - 1 ? _rules[SelectedIndex + 1] : null;
    private IEnumerable<RulesReferenceSource> SelectedSources => SelectedRule is null
        ? []
        : SelectedRule.AuthoritySourceIds
            .Select(id => _sources.GetValueOrDefault(id))
            .Where(source => source is not null)
            .Cast<RulesReferenceSource>();

    protected override async Task OnInitializedAsync() => await Load();

    private async Task Load()
    {
        _loading = true;
        _failed = false;
        try
        {
            var catalog = await Http.GetFromJsonAsync<RulesReferenceCatalog>(
                Navigation.ToAbsoluteUri("/data/rules-reference.json"));
            _rules = catalog?.Rules.OrderBy(rule => rule.Title).ToArray() ?? [];
            _sources = (catalog?.Sources ?? [])
                .ToDictionary(source => source.Id, StringComparer.OrdinalIgnoreCase);
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

    private static string RuleUrl(RulesReferenceEntry rule) => $"/game/reference/{rule.Key}";

    private static string Initial(string title) =>
        string.IsNullOrWhiteSpace(title) ? "#" : title[..1].ToUpperInvariant();

    private void ClearSelection() => Navigation.NavigateTo("/game/reference");

    private Task Retry() => Load();
}
