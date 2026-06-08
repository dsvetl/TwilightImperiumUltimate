using TwilightImperiumUltimate.Contracts.DTOs.Card;
using TwilightImperiumUltimate.Web.Components.Library.Models;

namespace TwilightImperiumUltimate.Web.Components.Library.Services;

public sealed class LibraryDataService(ITwilightImperiumApiHttpClient httpClient) : ILibraryDataService
{
    private readonly Dictionary<string, object> _cache = [];
    private readonly object _sync = new();

    public Task<IReadOnlyList<FactionDto>> GetFactionsAsync(bool refresh = false) =>
        GetListAsync<FactionDto>("factions", Paths.ApiPath_Factions, refresh);

    public Task<IReadOnlyList<TechnologyDto>> GetTechnologiesAsync(bool refresh = false) =>
        GetListAsync<TechnologyDto>("technologies", Paths.ApiPath_Technologies, refresh);

    public Task<IReadOnlyList<PlanetDto>> GetPlanetsAsync(bool refresh = false) =>
        GetListAsync<PlanetDto>("planets", Paths.ApiPath_Planets, refresh);

    public Task<IReadOnlyList<SystemTileDto>> GetSystemTilesAsync(bool refresh = false) =>
        GetListAsync<SystemTileDto>("system-tiles", Paths.ApiPath_SystemTiles, refresh);

    public Task<RulesLibrarySnapshot> GetRulesAsync(bool refresh = false) =>
        GetOrCreateAsync("rules-library", LoadRulesAsync, refresh);

    public Task<IReadOnlyList<LibraryCardItem>> GetCardsAsync(string type, bool refresh = false)
    {
        var normalized = NormalizeCardType(type);
        return normalized switch
        {
            "strategy" => GetCardListAsync<StrategyCardDto>(normalized, Paths.ApiPath_StrategyCards, card => card.StrategyCardName.ToString(), _ => "StrategyCard", refresh),
            "action" => GetCardListAsync<ActionCardDto>(normalized, Paths.ApiPath_ActionCards, card => card.ActionCardName.ToString(), _ => "ActionCard", refresh),
            "agenda" => GetCardListAsync<AgendaCardDto>(normalized, Paths.ApiPath_AgendaCards, card => card.AgendaCardName.ToString(), _ => "AgendaCard", refresh),
            "objective" => GetCardListAsync<ObjectiveCardDto>(normalized, Paths.ApiPath_ObjectiveCards, card => card.ObjectiveCardName.ToString(), card => $"ObjectiveCard{card.ObjectiveCardType}", refresh),
            "exploration" => GetCardListAsync<ExplorationCardDto>(normalized, Paths.ApiPath_ExplorationCards, card => card.ExplorationCardName.ToString(), _ => "ExplorationCard", refresh),
            "relic" => GetCardListAsync<RelicCardDto>(normalized, Paths.ApiPath_RelicCards, card => card.RelicCardName.ToString(), _ => "RelicCard", refresh),
            "frontier" => GetCardListAsync<FrontierCardDto>(normalized, Paths.ApiPath_FrontierCards, card => card.FrontierCardName.ToString(), _ => "FrontierCard", refresh),
            "promissory" => GetCardListAsync<PromissoryNoteCardDto>(normalized, Paths.ApiPath_PromissoryNoteCards, card => card.PromissoryNoteCardName.ToString(), _ => "PromissoryNoteCard", refresh),
            _ => GetCardListAsync<ActionCardDto>("action", Paths.ApiPath_ActionCards, card => card.ActionCardName.ToString(), _ => "ActionCard", refresh),
        };
    }

    private async Task<RulesLibrarySnapshot> LoadRulesAsync()
    {
        var rulesTask = LoadListAsync<RuleDto>(Paths.ApiPath_Rules);
        var faqsTask = LoadListAsync<FaqDto>(Paths.ApiPath_Faq);
        var factionsTask = LoadListAsync<FactionDto>(Paths.ApiPath_Factions);
        await Task.WhenAll(rulesTask, faqsTask, factionsTask);

        return new RulesLibrarySnapshot(
            await rulesTask,
            (await faqsTask).Where(faq => faq.FaqStatus == FaqStatus.Approved).ToArray(),
            await factionsTask);
    }

    private Task<IReadOnlyList<T>> GetListAsync<T>(string key, string path, bool refresh)
        where T : class =>
        GetOrCreateAsync(key, () => LoadListAsync<T>(path), refresh);

    private Task<IReadOnlyList<LibraryCardItem>> GetCardListAsync<T>(
        string type,
        string path,
        Func<T, string> getAssetKey,
        Func<T, string> getIconKey,
        bool refresh)
        where T : BaseCardDto =>
        GetOrCreateAsync(
            $"cards-{type}",
            async () => (IReadOnlyList<LibraryCardItem>)(await LoadListAsync<T>(path))
                .Select(card => new LibraryCardItem(
                    card.Id,
                    type,
                    getAssetKey(card),
                    getIconKey(card),
                    card.Name,
                    card.Text,
                    card.GameVersion))
                .ToArray(),
            refresh);

    private async Task<IReadOnlyList<T>> LoadListAsync<T>(string path)
        where T : class
    {
        var (response, statusCode) =
            await httpClient.GetAsync<ApiResponse<ItemListDto<T>>>(path);

        if (statusCode != HttpStatusCode.OK || response?.Data?.Items is null)
            throw new InvalidOperationException($"Library endpoint '{path}' returned {statusCode}.");

        return response.Data.Items.ToArray();
    }

    private async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, bool refresh)
    {
        Task<T> task;
        lock (_sync)
        {
            if (refresh)
                _cache.Remove(key);

            if (_cache.TryGetValue(key, out var cached))
            {
                task = (Task<T>)cached;
            }
            else
            {
                task = factory();
                _cache[key] = task;
            }
        }

        try
        {
            return await task;
        }
        catch
        {
            lock (_sync)
            {
                if (_cache.TryGetValue(key, out var cached) && ReferenceEquals(cached, task))
                    _cache.Remove(key);
            }

            throw;
        }
    }

    public static string NormalizeCardType(string? type) => type?.ToLowerInvariant() switch
    {
        "strategy" => "strategy",
        "agenda" => "agenda",
        "objective" => "objective",
        "exploration" => "exploration",
        "relic" => "relic",
        "frontier" => "frontier",
        "promissory" => "promissory",
        _ => "action",
    };
}
