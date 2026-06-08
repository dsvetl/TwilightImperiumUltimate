namespace TwilightImperiumUltimate.Web.Components.Shared.Navigation;

public sealed record NavigationItem(
    string Key,
    string Label,
    string Href,
    IReadOnlyList<NavigationItem>? Children = null,
    bool OpenInNewTab = false);
