namespace TwilightImperiumUltimate.Web.Components.Rules.Shared;

public partial class RulesCatalogLayout
{
    [Parameter, EditorRequired]
    public string ActiveSection { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;
}
