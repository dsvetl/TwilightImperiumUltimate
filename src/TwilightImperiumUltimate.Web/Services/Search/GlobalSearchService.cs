using System.Net.Http.Json;
using System.Text.RegularExpressions;
using TwilightImperiumUltimate.Web.Components.Rules.Reference;

namespace TwilightImperiumUltimate.Web.Services.Search;

public sealed partial class GlobalSearchService(
    ILibraryDataService library,
    HttpClient httpClient,
    NavigationManager navigation) : IGlobalSearchService
{
    private static readonly string[] CardTypes =
    [
        "action",
        "agenda",
        "objective",
        "exploration",
        "relic",
        "frontier",
        "promissory",
        "strategy",
    ];

    private readonly object _sync = new();
    private Lazy<Task<IReadOnlyList<SearchDocument>>>? _index;

    public Task<IReadOnlyList<SearchDocument>> GetIndexAsync()
    {
        Lazy<Task<IReadOnlyList<SearchDocument>>> index;
        lock (_sync)
        {
            _index ??= new Lazy<Task<IReadOnlyList<SearchDocument>>>(
                BuildIndexAsync,
                LazyThreadSafetyMode.ExecutionAndPublication);
            index = _index;
        }

        return AwaitAndResetOnFailure(index);
    }

    public void Invalidate()
    {
        lock (_sync)
            _index = null;
    }

    private async Task<IReadOnlyList<SearchDocument>> AwaitAndResetOnFailure(
        Lazy<Task<IReadOnlyList<SearchDocument>>> index)
    {
        try
        {
            return await index.Value;
        }
        catch
        {
            lock (_sync)
            {
                if (ReferenceEquals(_index, index))
                    _index = null;
            }

            throw;
        }
    }

    private async Task<IReadOnlyList<SearchDocument>> BuildIndexAsync()
    {
        var factionsTask = library.GetFactionsAsync();
        var technologiesTask = library.GetTechnologiesAsync();
        var planetsTask = library.GetPlanetsAsync();
        var tilesTask = library.GetSystemTilesAsync();
        var rulesTask = library.GetRulesAsync();
        var referenceTask = httpClient.GetFromJsonAsync<RulesReferenceCatalog>(
            navigation.ToAbsoluteUri("/data/rules-reference.json"));
        var cardTasks = CardTypes.Select(type => library.GetCardsAsync(type)).ToArray();
        var resourcesTask = httpClient.GetFromJsonAsync<GameResourceSearchItem[]>(
            navigation.ToAbsoluteUri("/data/game-resources.json"));

        await Task.WhenAll(
            [factionsTask, technologiesTask, planetsTask, tilesTask, rulesTask, referenceTask, resourcesTask, .. cardTasks]);

        var documents = new List<SearchDocument>();
        AddFactions(documents, await factionsTask);
        AddTechnologies(documents, await technologiesTask);
        AddPlanets(documents, await planetsTask);
        AddSystemTiles(documents, await tilesTask);
        AddRules(documents, await referenceTask, await rulesTask);
        AddResources(documents, await resourcesTask ?? []);

        foreach (var cards in cardTasks)
            AddCards(documents, await cards);

        return documents;
    }

    private static void AddFactions(
        List<SearchDocument> documents,
        IReadOnlyList<FactionDto> factions)
    {
        documents.AddRange(factions.Select(faction => new SearchDocument(
            $"faction-{faction.Id}",
            "Factions",
            LibraryFormatting.Faction(faction.FactionName),
            LibraryFormatting.Version(faction.GameVersion),
            ToExcerpt(faction.FactionName.GetFactionUIText(FactionResourceType.Quote)),
            $"/game/factions?faction={faction.FactionName}",
            ToPlainText(string.Join(' ',
                faction.FactionName.GetFactionUIText(FactionResourceType.Ability),
                faction.FactionName.GetFactionUIText(FactionResourceType.Lore),
                faction.FactionName.GetFactionUIText(FactionResourceType.Notes),
                faction.SystemInfo,
                faction.SystemStats)))));
    }

    private static void AddTechnologies(
        List<SearchDocument> documents,
        IReadOnlyList<TechnologyDto> technologies)
    {
        documents.AddRange(technologies.Select(technology => new SearchDocument(
            $"technology-{technology.Id}",
            "Technologies",
            technology.Name,
            LibraryFormatting.Identifier(technology.Type.ToString()),
            ToExcerpt(technology.Text),
            $"/game/technologies?type={technology.Type}",
            $"{technology.TechnologyName} {technology.FactionName} {technology.GameVersion}")));
    }

    private static void AddPlanets(
        List<SearchDocument> documents,
        IReadOnlyList<PlanetDto> planets)
    {
        documents.AddRange(planets.Select(planet => new SearchDocument(
            $"planet-{planet.Id}",
            "Planets",
            LibraryFormatting.Identifier(planet.PlanetName.ToString()),
            LibraryFormatting.Identifier(planet.PlanetTrait.ToString()),
            $"{planet.Resources} resources, {planet.Influence} influence",
            $"/game/board?planet={planet.PlanetName}",
            $"{planet.TechnologySkip} {planet.GameVersion}")));
    }

    private static void AddSystemTiles(
        List<SearchDocument> documents,
        IReadOnlyList<SystemTileDto> tiles)
    {
        documents.AddRange(tiles
            .Where(tile => !string.IsNullOrWhiteSpace(tile.SystemTileCode))
            .Select(tile => new SearchDocument(
                $"system-tile-{tile.Id}",
                "System Tiles",
                $"System {tile.SystemTileCode}",
                LibraryFormatting.Identifier(tile.SystemTileCategory.ToString()),
                string.Join(", ", tile.Planets.Select(planet =>
                    LibraryFormatting.Identifier(planet.PlanetName.ToString()))),
                $"/game/board?tile={Uri.EscapeDataString(tile.SystemTileCode)}",
                $"{tile.SystemTileName} {tile.AnomalyName} {tile.GameVersion}")));
    }

    private static void AddRules(
        List<SearchDocument> documents,
        RulesReferenceCatalog? catalog,
        RulesLibrarySnapshot snapshot)
    {
        documents.AddRange((catalog?.Rules ?? []).Select(rule => new SearchDocument(
            $"rule-{rule.Key}",
            "Rules",
            rule.Title,
            "Rules reference",
            ToExcerpt(rule.ContentHtml),
            $"/game/reference/{rule.Key}",
            ToPlainText($"{rule.ContentHtml} {rule.NotesHtml}"))));

        var factionKeys = snapshot.Factions
            .Select(faction => faction.FactionName.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        documents.AddRange(snapshot.Faqs.Select(faq => new SearchDocument(
            $"faq-{faq.Id}",
            "FAQ",
            RulesContentCatalog.GetComponentTitle(faq.ComponentName),
            ToPlainText(faq.QuestionEnglish),
            ToExcerpt(faq.AnswerEnglish),
            factionKeys.Contains(faq.ComponentName)
                ? $"/game/factions?faction={Uri.EscapeDataString(faq.ComponentName)}&info=faq"
                : RulesContentCatalog.GetCanonicalComponentUrl(faq.ComponentName),
            ToPlainText($"{faq.QuestionEnglish} {faq.AnswerEnglish} {faq.QuestionCzech} {faq.AnswerCzech}"))));
    }

    private static void AddCards(
        List<SearchDocument> documents,
        IReadOnlyList<LibraryCardItem> cards)
    {
        documents.AddRange(cards.Select(card => new SearchDocument(
            $"card-{card.Type}-{card.Id}",
            "Cards",
            card.Name,
            LibraryFormatting.Identifier(card.Type),
            ToExcerpt(card.Text),
            $"/game/cards?type={card.Type}",
            $"{card.Version} {card.Type}")));
    }

    private static void AddResources(
        List<SearchDocument> documents,
        IReadOnlyList<GameResourceSearchItem> resources)
    {
        documents.AddRange(resources.Select(resource => new SearchDocument(
            $"resource-{resource.Id}",
            "Resources",
            resource.Title,
            resource.Type,
            ToExcerpt(resource.Description),
            ResourceUrl(resource.Type),
            ToPlainText($"{resource.Description} {resource.Type} {resource.Version}"))));
    }

    private static string ResourceUrl(string type)
    {
        var normalizedType = WhitespaceRegex()
            .Replace(type.Trim().ToLowerInvariant(), "-");
        return string.IsNullOrWhiteSpace(normalizedType)
            ? "/game/resources"
            : $"/game/resources?type={Uri.EscapeDataString(normalizedType)}";
    }

    private static string ToExcerpt(string value)
    {
        var plainText = ToPlainText(value);
        return plainText.Length <= 180 ? plainText : $"{plainText[..177].TrimEnd()}...";
    }

    private static string ToPlainText(string value) =>
        WhitespaceRegex().Replace(HtmlRegex().Replace(WebUtility.HtmlDecode(value ?? string.Empty), " "), " ").Trim();

    [GeneratedRegex("<.*?>")]
    private static partial Regex HtmlRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespaceRegex();

    private sealed class GameResourceSearchItem
    {
        public string Id { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string Version { get; init; } = string.Empty;
    }
}
