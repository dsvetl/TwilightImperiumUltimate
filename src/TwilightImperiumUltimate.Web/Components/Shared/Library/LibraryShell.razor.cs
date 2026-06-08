namespace TwilightImperiumUltimate.Web.Components.Shared.Library;

public partial class LibraryShell
{
    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;
}
