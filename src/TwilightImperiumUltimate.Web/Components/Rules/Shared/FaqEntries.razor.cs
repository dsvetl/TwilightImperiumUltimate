namespace TwilightImperiumUltimate.Web.Components.Rules.Shared;

public partial class FaqEntries
{
    [Parameter, EditorRequired]
    public IReadOnlyList<FaqDto> Items { get; set; } = [];
}
