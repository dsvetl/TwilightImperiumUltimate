namespace TwilightImperiumUltimate.Web.Components.Library.Models;

public sealed record RulesLibrarySnapshot(
    IReadOnlyList<RuleDto> Rules,
    IReadOnlyList<FaqDto> Faqs,
    IReadOnlyList<FactionDto> Factions);
