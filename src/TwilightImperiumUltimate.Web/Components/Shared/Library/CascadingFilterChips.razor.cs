namespace TwilightImperiumUltimate.Web.Components.Shared.Library;

public partial class CascadingFilterChips
{
    [Parameter, EditorRequired]
    public IReadOnlyList<FilterChipItem> PrimaryItems { get; set; } = [];

    [Parameter]
    public string PrimaryValue { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> PrimaryValueChanged { get; set; }

    [Parameter]
    public IReadOnlyList<FilterChipItem> SecondaryItems { get; set; } = [];

    [Parameter]
    public string SecondaryValue { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> SecondaryValueChanged { get; set; }

    [Parameter]
    public string AriaLabel { get; set; } = "Filters";

    [Parameter]
    public string SecondaryAriaLabel { get; set; } = "Dependent filters";

    private bool ShowSecondary =>
        SecondaryItems.Count(item => !string.IsNullOrWhiteSpace(item.Value) && !item.Disabled) >= 2;
}
