namespace TwilightImperiumUltimate.Web.Components.Rules.Reference;

public partial class AlphabetIndex
{
    private static readonly IReadOnlyList<string> Letters = Enumerable
        .Range('A', 26)
        .Select(value => ((char)value).ToString())
        .ToArray();

    [Parameter, EditorRequired]
    public IReadOnlySet<string> AvailableLetters { get; set; } =
        new HashSet<string>(StringComparer.Ordinal);

    public static string AnchorId(string letter) => $"rule-letter-{letter.ToLowerInvariant()}";
}
