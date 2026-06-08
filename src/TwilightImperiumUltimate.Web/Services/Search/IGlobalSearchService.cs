namespace TwilightImperiumUltimate.Web.Services.Search;

public interface IGlobalSearchService
{
    Task<IReadOnlyList<SearchDocument>> GetIndexAsync();

    void Invalidate();
}
