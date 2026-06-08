using Microsoft.AspNetCore.Components.Routing;

namespace TwilightImperiumUltimate.Web.Components.Shared.Navigation;

public partial class NavigationGroup
{
    [Parameter, EditorRequired]
    public NavigationItem Item { get; set; } = default!;

    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<string> OnToggle { get; set; }

    [Parameter]
    public EventCallback OnNavigate { get; set; }

    private bool HasChildren => Item.Children?.Count > 0;

    private NavLinkMatch LinkMatch =>
        Item.Href == "/" ? NavLinkMatch.All : NavLinkMatch.Prefix;

    private Task Toggle() => OnToggle.InvokeAsync(Item.Key);

    private Task Navigate() => OnNavigate.InvokeAsync();
}
