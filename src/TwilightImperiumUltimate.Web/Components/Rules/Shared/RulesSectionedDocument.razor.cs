using Microsoft.JSInterop;

namespace TwilightImperiumUltimate.Web.Components.Rules.Shared;

public partial class RulesSectionedDocument
{
    private ElementReference _root;
    private IJSObjectReference? _module;
    private string? _indexedHtml;

    [Parameter, EditorRequired]
    public string Html { get; set; } = string.Empty;

    [Parameter]
    public string AriaLabel { get; set; } = "Document sections";

    [Parameter]
    public bool DetailedIndex { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Html == _indexedHtml)
            return;

        try
        {
            _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./Components/Rules/Shared/RulesSectionedDocument.razor.js");
            await _module.InvokeVoidAsync("buildSectionIndex", _root, DetailedIndex);
            _indexedHtml = Html;
        }
        catch (JSException)
        {
            // Navigation is progressive enhancement; the document remains readable.
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
            await _module.DisposeAsync();
    }
}