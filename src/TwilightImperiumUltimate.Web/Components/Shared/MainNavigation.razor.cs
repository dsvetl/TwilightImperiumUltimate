namespace TwilightImperiumUltimate.Web.Components.Shared;

public partial class MainNavigation
{
    private string? _openKey;
    private bool _mobileOpen;
    private bool _searchOpen;

    private void ToggleMobile()
    {
        _mobileOpen = !_mobileOpen;
        if (!_mobileOpen)
            _openKey = null;
    }

    private void ToggleGroup(string key) => _openKey = _openKey == key ? null : key;

    private void CloseMobile() => _mobileOpen = false;

    private void CloseAll()
    {
        _mobileOpen = false;
        _openKey = null;
    }

    private void OpenSearch()
    {
        CloseAll();
        _searchOpen = true;
    }

    private void CloseSearch() => _searchOpen = false;
}
