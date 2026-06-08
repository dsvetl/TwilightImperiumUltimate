namespace TwilightImperiumUltimate.Web.Components.Library;

public static class ContentSourceFilters
{
    public static readonly IReadOnlyList<FilterChipItem> Sources =
    [
        new("all", "All"),
        new("official", "Official"),
        new("community", "Community"),
    ];

    public static string NormalizeSource(string? source) => source?.ToLowerInvariant() switch
    {
        "official" => "official",
        "community" => "community",
        _ => "all",
    };

    public static IReadOnlyList<GameVersion> AvailableVersions(
        IEnumerable<GameVersion> versions,
        string source) =>
        source == "all"
            ? []
            : versions
                .Where(IsBrowsable)
                .Where(version => LibraryFormatting.SourceValue(version) == source)
                .Distinct()
                .OrderBy(LibraryFormatting.VersionOrder)
                .ToArray();

    public static IReadOnlyList<FilterChipItem> VersionItems(
        IEnumerable<GameVersion> versions,
        string source,
        IEnumerable<GameVersion>? availableVersions = null)
    {
        var stableVersions = AvailableVersions(versions, source);
        var available = AvailableVersions(availableVersions ?? versions, source).ToHashSet();
        if (available.Count <= 1)
            return [];

        return
        [
            new(string.Empty, "All"),
            .. stableVersions.Select(version => new FilterChipItem(
                version.ToString(),
                LibraryFormatting.Version(version),
                !available.Contains(version))),
        ];
    }

    public static string NormalizeVersion(
        string? version,
        IEnumerable<GameVersion> versions,
        string source,
        IEnumerable<GameVersion>? availableVersions = null)
    {
        var requested = version ?? string.Empty;
        var available = AvailableVersions(availableVersions ?? versions, source).ToHashSet();
        if (available.Count <= 1)
            return string.Empty;

        return AvailableVersions(versions, source)
            .Cast<GameVersion?>()
            .FirstOrDefault(item =>
                item is not null
                && available.Contains(item.Value)
                && item.Value.ToString().Equals(requested, StringComparison.OrdinalIgnoreCase))
            ?.ToString() ?? string.Empty;
    }

    public static bool Matches(
        GameVersion version,
        string source,
        string selectedVersion) =>
        IsBrowsable(version)
        && (source == "all"
        || (LibraryFormatting.SourceValue(version) == source
            && (string.IsNullOrWhiteSpace(selectedVersion)
                || version.ToString().Equals(selectedVersion, StringComparison.OrdinalIgnoreCase))));

    private static bool IsBrowsable(GameVersion version) =>
        version is not GameVersion.Deprecated and not GameVersion.Custom;
}
