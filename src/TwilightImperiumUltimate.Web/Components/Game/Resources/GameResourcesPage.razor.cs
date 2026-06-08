using System.Net.Http.Json;

namespace TwilightImperiumUltimate.Web.Components.Game.Resources;

public partial class GameResourcesPage
{
    private IReadOnlyList<GameResource> _resources = [];
    private string _type = "all";
    private bool _loading = true;
    private bool _failed;

    [Parameter]
    public string? Type { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private ILogger<GameResourcesPage> Logger { get; set; } = default!;

    private IReadOnlyList<FilterChipItem> TypeFilters =>
        [new("all", "All"), .. _resources
            .Select(resource => resource.Type)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(type => type)
            .Select(type => new FilterChipItem(Value(type), type))];

    private IEnumerable<GameResource> VisibleResources => _resources
        .Where(resource => _type == "all" || Value(resource.Type) == _type)
        .OrderBy(resource => resource.Type)
        .ThenBy(resource => resource.Title);

    protected override async Task OnInitializedAsync() => await Load();

    protected override void OnParametersSet()
    {
        _type = Normalize(Type, TypeFilters, "all");
    }

    private async Task Load()
    {
        _loading = true;
        _failed = false;
        try
        {
            _resources = await Http.GetFromJsonAsync<GameResource[]>(
                Navigation.ToAbsoluteUri("/data/game-resources.json")) ?? [];
            _type = Normalize(Type, TypeFilters, "all");
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to load the game resource catalog.");
            _failed = true;
        }
        finally
        {
            _loading = false;
        }
    }

    private void SetType(string value)
    {
        _type = value;
        UpdateUrl();
    }

    private void UpdateUrl()
    {
        var uri = Navigation.GetUriWithQueryParameters(
            Navigation.ToAbsoluteUri("/game/resources").ToString(),
            new Dictionary<string, object?>
            {
                ["type"] = _type == "all" ? null : _type,
            });
        Navigation.NavigateTo(uri, replace: true);
    }

    private static string Normalize(
        string? requested,
        IReadOnlyList<FilterChipItem> options,
        string fallback)
    {
        var value = Value(requested ?? fallback);
        return options.Any(option => option.Value == value) ? value : fallback;
    }

    private static string Value(string value) => value
        .Trim()
        .ToLowerInvariant()
        .Replace(' ', '-');
}
