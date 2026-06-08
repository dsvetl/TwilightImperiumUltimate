namespace TwilightImperiumUltimate.Web.Components.Shared.Library;

public partial class FilterChips
{
    [Parameter, EditorRequired]
    public IReadOnlyList<FilterChipItem> Items { get; set; } = [];

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public string AriaLabel { get; set; } = "Filters";

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    private Task Select(string value) => ValueChanged.InvokeAsync(value);
}

public sealed record FilterChipItem(string Value, string Label, bool Disabled = false);
