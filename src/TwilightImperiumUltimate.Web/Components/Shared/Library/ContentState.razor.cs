namespace TwilightImperiumUltimate.Web.Components.Shared.Library;

public partial class ContentState
{
    [Parameter]
    public string Mark { get; set; } = "◈";

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? Action { get; set; }
}
