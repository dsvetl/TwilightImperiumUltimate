namespace TwilightImperiumUltimate.Web.Components.Game.Cards;

public partial class LibraryCard
{
    private bool _showNotes;
    private bool _showDetails;

    [Parameter, EditorRequired]
    public LibraryCardItem Card { get; set; } = default!;

    [Parameter]
    public IReadOnlyList<FaqDto> Faqs { get; set; } = [];

    [Parameter]
    public string Notes { get; set; } = string.Empty;

    private bool HasDetails => !string.IsNullOrWhiteSpace(Card.Text);
    private bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

    private void ToggleNotes() => _showNotes = !_showNotes;
    private void ToggleDetails() => _showDetails = !_showDetails;
}
