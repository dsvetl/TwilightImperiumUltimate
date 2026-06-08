namespace TwilightImperiumUltimate.Web.Services.Search;

public sealed record SearchDocument(
    string Id,
    string Category,
    string Title,
    string Subtitle,
    string Excerpt,
    string Url,
    string Keywords);
