namespace TwilightImperiumUltimate.Web.Components.Shared.Library;

public partial class OverviewIntro
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Description { get; set; } = string.Empty;

    [Parameter]
    public string Image { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? Actions { get; set; }
}
