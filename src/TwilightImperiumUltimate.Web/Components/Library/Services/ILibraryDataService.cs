using TwilightImperiumUltimate.Web.Components.Library.Models;

namespace TwilightImperiumUltimate.Web.Components.Library.Services;

public interface ILibraryDataService
{
    Task<IReadOnlyList<FactionDto>> GetFactionsAsync(bool refresh = false);

    Task<IReadOnlyList<TechnologyDto>> GetTechnologiesAsync(bool refresh = false);

    Task<IReadOnlyList<PlanetDto>> GetPlanetsAsync(bool refresh = false);

    Task<IReadOnlyList<SystemTileDto>> GetSystemTilesAsync(bool refresh = false);

    Task<IReadOnlyList<LibraryCardItem>> GetCardsAsync(string type, bool refresh = false);

    Task<RulesLibrarySnapshot> GetRulesAsync(bool refresh = false);
}
