namespace TwilightImperiumUltimate.Web.Components.Game.Shared;

public partial class GameCatalogLayout
{
    [Parameter, EditorRequired]
    public string ActiveSection { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;
}
