using TwilightImperiumUltimate.Web.Services.MapGenerators;

namespace TwilightImperiumUltimate.Web.Components.Factions;

public partial class FactionIconRow : TwilightImperiumBaseComponenet
{
    private List<FactionModel>? _factions = [];
    private string _lastInitializedSelection = string.Empty;

    [Parameter]
    public EventCallback<FactionModel> OnFactionClickGetFaction { get; set; }

    [Parameter]
    public EventCallback<IReadOnlyCollection<FactionModel>> OnInitializeGetFactions { get; set; }

    [Parameter]
    public bool EnableBanMode { get; set; }

    [Parameter]
    public bool BanAllFactions { get; set; }

    [Parameter]
    public bool ShowDiscordantStars { get; set; }

    [Parameter]
    public bool ShowBaseGame { get; set; } = true;

    [Parameter]
    public List<FactionModel> ProvidedFactions { get; set; } = [];

    [Parameter]
    public string Faction { get; set; } = string.Empty;

    [Parameter]
    public GameVersion? SelectedVersion { get; set; }

    [Parameter]
    public IReadOnlyCollection<GameVersion> VisibleVersions { get; set; } = [];

    [Parameter]
    public bool ShowSelection { get; set; }

    public IReadOnlyCollection<FactionModel>? Factions => _factions;

    [Inject]
    private IMapGeneratorSettingsService MapGeneratorSettingsService { get; set; } = default!;

    public void RefreshFactions()
    {
        _factions = MapGeneratorSettingsService.FactionsForMapGenerator;
        StateHasChanged();
    }

    public void SetAllFactionsBanStatus(bool banStatus)
    {
        _factions?.ForEach(x => x.Banned = banStatus);
    }

    protected override async Task OnInitializedAsync()
    {
        // This component is also used by draft and map-generator workflows.
        if (ProvidedFactions.Count != 0)
        {
            _factions = ProvidedFactions;
            await OnInitializeGetFactions.InvokeAsync(Factions);
            MapGeneratorSettingsService.FactionsForMapGenerator = ProvidedFactions;
            await InitializeSelection();
            return;
        }

        await InitializeFactions();
    }

    protected override async Task OnParametersSetAsync()
    {
        await InitializeSelection();
    }

    private async Task FactionClicked(FactionModel selectedFaction)
    {
        if (EnableBanMode)
            selectedFaction.Banned = !selectedFaction.Banned;

        Faction = selectedFaction.FactionName.ToString();
        _lastInitializedSelection = GetSelectionKey(selectedFaction);
        await OnFactionClickGetFaction.InvokeAsync(selectedFaction);
    }

    private async Task InitializeSelection()
    {
        var available = GetDisplayedFactions();
        if (available.Count == 0)
            return;

        var selectedFaction = Enum.TryParse<FactionName>(Faction, out var requestedFaction)
            ? available.FirstOrDefault(x => x.FactionName == requestedFaction)
            : null;
        selectedFaction ??= available[0];

        var selectionKey = GetSelectionKey(selectedFaction);
        if (_lastInitializedSelection == selectionKey)
            return;

        Faction = selectedFaction.FactionName.ToString();
        _lastInitializedSelection = selectionKey;
        await OnFactionClickGetFaction.InvokeAsync(selectedFaction);
    }

    private async Task InitializeFactions()
    {
        var (response, statusCode) = await HttpClient.GetAsync<ApiResponse<ItemListDto<FactionDto>>>(Paths.ApiPath_Factions);
        if (statusCode != HttpStatusCode.OK)
            return;

        _factions = Mapper.Map<List<FactionModel>>(response!.Data!.Items);
        await OnInitializeGetFactions.InvokeAsync(Factions);

        if (BanAllFactions)
            SetAllFactionsBanStatus(true);

        await InitializeSelection();
    }

    private List<FactionModel> GetBaseGameFactions()
    {
        return _factions?.Where(x => x.GameVersion != GameVersion.DiscordantStars).ToList() ?? [];
    }

    private List<FactionModel> GetDiscordantStarsFactions()
    {
        return _factions?.Where(x => x.GameVersion == GameVersion.DiscordantStars).ToList() ?? [];
    }

    private List<FactionModel> GetVersionFactions(GameVersion version)
    {
        return _factions?
            .Where(x => x.GameVersion == version)
            .OrderBy(x => x.FactionName)
            .ToList() ?? [];
    }

    private List<FactionModel> GetDisplayedFactions()
    {
        if (SelectedVersion is not null)
            return GetVersionFactions(SelectedVersion.Value);

        if (VisibleVersions.Count > 0)
        {
            return _factions?
                .Where(x => VisibleVersions.Contains(x.GameVersion))
                .OrderBy(x => x.FactionName)
                .ToList() ?? [];
        }

        return _factions?
            .Where(x => (ShowBaseGame && x.GameVersion != GameVersion.DiscordantStars)
                || (ShowDiscordantStars && x.GameVersion == GameVersion.DiscordantStars))
            .ToList() ?? [];
    }

    private bool IsSelected(FactionModel faction) =>
        faction.FactionName.ToString().Equals(Faction, StringComparison.Ordinal);

    private string GetSelectionKey(FactionModel faction) =>
        $"{SelectedVersion}:{faction.FactionName}";
}
