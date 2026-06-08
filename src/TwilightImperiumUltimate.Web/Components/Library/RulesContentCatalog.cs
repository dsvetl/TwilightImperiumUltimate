using System.Collections;
using System.Globalization;
using System.Net;
using System.Resources;
using System.Text.RegularExpressions;

namespace TwilightImperiumUltimate.Web.Components.Library;

public static class RulesContentCatalog
{
    private static readonly ResourceManager CardNames =
        new(Paths.ResourceNamespace_CardNames, typeof(Program).Assembly);
    private static readonly Dictionary<string, IReadOnlyDictionary<string, string>> ComponentNotes = [];
    private static readonly object ComponentNotesSync = new();

    public static string GetRuleTitle(RuleCategory category)
    {
        var title = category.GetRuleTitleUIText();
        return string.IsNullOrWhiteSpace(title)
            ? LibraryFormatting.Identifier(category.ToString())
            : title;
    }

    public static string GetRuleContent(RuleCategory category) =>
        category.GetRuleUIText();

    public static string GetRuleNotes(RuleCategory category) =>
        category.GetRuleUINoteText();

    public static string GetFactionNotes(FactionName faction) =>
        faction.GetFactionUIText(FactionResourceType.Notes);

    public static IReadOnlyDictionary<string, string> GetComponentNotes()
    {
        var cultureName = CultureInfo.CurrentUICulture.Name;
        lock (ComponentNotesSync)
        {
            if (ComponentNotes.TryGetValue(cultureName, out var cached))
                return cached;
        }

        var resourceSet = RuleNotes.ResourceManager.GetResourceSet(
            CultureInfo.CurrentUICulture,
            createIfNotExists: true,
            tryParents: true);
        var notes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (resourceSet is not null)
        {
            foreach (DictionaryEntry entry in resourceSet)
            {
                if (entry.Key is string key
                    && entry.Value is string value
                    && HasVisibleContent(value))
                {
                    notes[key] = value;
                }
            }
        }

        lock (ComponentNotesSync)
        {
            ComponentNotes[cultureName] = notes;
            return notes;
        }
    }

    private static bool HasVisibleContent(string value)
    {
        var plainText = Regex.Replace(value, "<[^>]+>", " ");
        return !string.IsNullOrWhiteSpace(WebUtility.HtmlDecode(plainText));
    }

    public static string GetComponentTitle(string key)
    {
        if (TryGetDisplayName<ActionCardName>(key, out var title)
            || TryGetDisplayName<AgendaCardName>(key, out title)
            || TryGetDisplayName<ExplorationCardName>(key, out title)
            || TryGetDisplayName<FrontierCardName>(key, out title)
            || TryGetDisplayName<ObjectiveCardName>(key, out title)
            || TryGetDisplayName<PromissoryNoteCardName>(key, out title)
            || TryGetDisplayName<RelicCardName>(key, out title)
            || TryGetDisplayName<StrategyCardName>(key, out title)
            || TryGetDisplayName<TechnologyName>(key, out title)
            || TryGetDisplayName<FlagshipName>(key, out title)
            || TryGetDisplayName<SpecialComponentName>(key, out title))
        {
            return title;
        }

        return LibraryFormatting.Identifier(key);
    }

    public static RulesComponentDescriptor GetComponentDescriptor(string key)
    {
        if (Enum.TryParse<TechnologyName>(key, ignoreCase: true, out _))
            return new("technologies", string.Empty, "Technologies", null);
        if (Enum.TryParse<RelicCardName>(key, ignoreCase: true, out _))
            return new("relics", "relic", "Relics", "RelicCard");
        if (Enum.TryParse<ObjectiveCardName>(key, ignoreCase: true, out _))
            return new("objectives", "objective", "Objectives", null);
        if (Enum.TryParse<ActionCardName>(key, ignoreCase: true, out _))
            return new("cards", "action", "Action Cards", "ActionCard");
        if (Enum.TryParse<AgendaCardName>(key, ignoreCase: true, out _))
            return new("cards", "agenda", "Agenda Cards", "AgendaCard");
        if (Enum.TryParse<ExplorationCardName>(key, ignoreCase: true, out _))
            return new("cards", "exploration", "Exploration Cards", "ExplorationCard");
        if (Enum.TryParse<FrontierCardName>(key, ignoreCase: true, out _))
            return new("cards", "frontier", "Frontier Cards", "FrontierCard");
        if (Enum.TryParse<PromissoryNoteCardName>(key, ignoreCase: true, out _))
            return new("cards", "promissory", "Promissory Notes", "PromissoryNoteCard");
        if (Enum.TryParse<StrategyCardName>(key, ignoreCase: true, out _))
            return new("cards", "strategy", "Strategy Cards", "StrategyCard");

        return new("cards", "other", "Other Components", "ActionCard");
    }

    public static string GetCanonicalComponentUrl(string key)
    {
        var descriptor = GetComponentDescriptor(key);
        if (descriptor.CategoryValue == "technologies")
            return "/game/technologies";

        var cardType = descriptor.CategoryValue switch
        {
            "relics" => "relic",
            "objectives" => "objective",
            _ => descriptor.SubtypeValue switch
            {
                "agenda" => "agenda",
                "exploration" => "exploration",
                "frontier" => "frontier",
                "promissory" => "promissory",
                "strategy" => "strategy",
                _ => "action",
            },
        };
        return cardType == "action"
            ? "/game/cards"
            : $"/game/cards?type={cardType}";
    }

    private static bool TryGetDisplayName<TEnum>(string key, out string title)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(key, ignoreCase: true, out var value))
        {
            title = CardNames.GetString(
                $"{typeof(TEnum).Name}_{value}",
                CultureInfo.CurrentUICulture) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(title))
                return true;
        }

        title = string.Empty;
        return false;
    }
}

public sealed record RulesComponentDescriptor(
    string CategoryValue,
    string SubtypeValue,
    string SectionName,
    string? CardIconKey);
