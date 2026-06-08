using System.Text;

namespace TwilightImperiumUltimate.Web.Components.Library.Models;

public sealed class RulingsCatalog
{
    public string SourceRevision { get; init; } = string.Empty;

    public IReadOnlyList<RulingsEntry> Items { get; init; } = [];
}

public sealed class RulingsEntry
{
    public string Key { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public IReadOnlyList<RulingsSection> Sections { get; init; } = [];
}

public sealed class RulingsSection
{
    public string Title { get; init; } = string.Empty;

    public string Html { get; init; } = string.Empty;
}

public static class RulingsCatalogIndex
{
    public static IReadOnlyDictionary<string, string> ForItem(
        RulingsCatalog? catalog,
        string itemKey)
    {
        var item = catalog?.Items.FirstOrDefault(entry =>
            entry.Key.Equals(itemKey, StringComparison.OrdinalIgnoreCase));
        return item?.Sections
            .Where(section => !string.IsNullOrWhiteSpace(section.Html))
            .GroupBy(section => Normalize(section.Title), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => string.Join(string.Empty, group.Select(section => section.Html)),
                StringComparer.Ordinal)
            ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public static string Normalize(string value) =>
        new(value
            .Normalize(NormalizationForm.FormD)
            .Where(character => char.IsLetterOrDigit(character))
            .Select(char.ToLowerInvariant)
            .ToArray());
}
