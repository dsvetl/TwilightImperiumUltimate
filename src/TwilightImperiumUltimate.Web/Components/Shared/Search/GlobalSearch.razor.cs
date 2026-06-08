using Microsoft.AspNetCore.Components.Web;
using TwilightImperiumUltimate.Web.Services.Search;

namespace TwilightImperiumUltimate.Web.Components.Shared.Search;

public partial class GlobalSearch
{
    private ElementReference _input;
    private IReadOnlyList<SearchDocument> _documents = [];
    private string _query = string.Empty;
    private bool _loading;
    private bool _failed;
    private bool _focusPending;

    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Inject]
    private IGlobalSearchService SearchService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private IReadOnlyList<SearchDocument> Results =>
        _query.Trim().Length < 2
            ? []
            : _documents
                .Select(document => new
                {
                    Document = document,
                    Score = GetScore(document, _query.Trim()),
                })
                .Where(item => item.Score > 0)
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Document.Title)
                .Take(30)
                .Select(item => item.Document)
                .ToArray();

    protected override async Task OnParametersSetAsync()
    {
        if (!Open)
            return;

        _focusPending = true;
        if (_documents.Count == 0 && !_loading)
            await LoadAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_focusPending && Open)
        {
            _focusPending = false;
            await _input.FocusAsync();
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _failed = false;
        try
        {
            _documents = await SearchService.GetIndexAsync();
        }
        catch
        {
            _failed = true;
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task Retry()
    {
        SearchService.Invalidate();
        await LoadAsync();
    }

    private void OnInput(ChangeEventArgs args) => _query = args.Value?.ToString() ?? string.Empty;

    private void Navigate(string url)
    {
        NavigationManager.NavigateTo(url);
        _ = Close();
    }

    private Task Close() => OnClose.InvokeAsync();

    private Task HandleKeyDown(KeyboardEventArgs args) =>
        args.Key == "Escape" ? Close() : Task.CompletedTask;

    private static int GetScore(SearchDocument document, string query)
    {
        if (document.Title.Equals(query, StringComparison.CurrentCultureIgnoreCase))
            return 100;
        if (document.Title.StartsWith(query, StringComparison.CurrentCultureIgnoreCase))
            return 75;
        if (document.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase))
            return 50;
        if (document.Subtitle.Contains(query, StringComparison.CurrentCultureIgnoreCase))
            return 30;
        if (document.Keywords.Contains(query, StringComparison.CurrentCultureIgnoreCase))
            return 20;
        return document.Excerpt.Contains(query, StringComparison.CurrentCultureIgnoreCase) ? 10 : 0;
    }
}
