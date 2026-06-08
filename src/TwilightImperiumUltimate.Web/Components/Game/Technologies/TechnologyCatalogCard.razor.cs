namespace TwilightImperiumUltimate.Web.Components.Game.Technologies;

public partial class TechnologyCatalogCard
{
    private bool _showDetails;
    private bool _showNotes;

    [Parameter, EditorRequired]
    public TechnologyDto Technology { get; set; } = default!;

    [Parameter]
    public IReadOnlyList<FaqDto> Faqs { get; set; } = [];

    [Parameter]
    public string Notes { get; set; } = string.Empty;

    private bool HasDetails => !string.IsNullOrWhiteSpace(Technology.Text);
    private bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

    private void ToggleDetails() => _showDetails = !_showDetails;
    private void ToggleNotes() => _showNotes = !_showNotes;
}
