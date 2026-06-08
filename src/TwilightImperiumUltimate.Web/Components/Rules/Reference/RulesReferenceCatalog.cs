namespace TwilightImperiumUltimate.Web.Components.Rules.Reference;

public sealed class RulesReferenceCatalog
{
    public int SchemaVersion { get; init; }

    public string SourceRevision { get; init; } = string.Empty;

    public IReadOnlyList<RulesReferenceSource> Sources { get; init; } = [];

    public IReadOnlyList<RulesReferenceEntry> Rules { get; init; } = [];
}

public sealed class RulesReferenceSource
{
    public string Id { get; init; } = string.Empty;

    public string Kind { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Version { get; init; }

    public string? Url { get; init; }

    public string? Repository { get; init; }

    public string? Revision { get; init; }
}

public sealed class RulesReferenceEntry
{
    public string Key { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string ContentHtml { get; init; } = string.Empty;

    public string NotesHtml { get; init; } = string.Empty;

    public int? LegacyId { get; init; }

    public string? LegacyCategory { get; init; }

    public IReadOnlyList<string> RelatedKeys { get; init; } = [];

    public IReadOnlyList<string> SourceIds { get; init; } = [];

    public IReadOnlyList<string> AuthoritySourceIds { get; init; } = [];
}
