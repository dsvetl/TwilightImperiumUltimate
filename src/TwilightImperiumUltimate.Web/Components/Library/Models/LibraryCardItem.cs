namespace TwilightImperiumUltimate.Web.Components.Library.Models;

public sealed record LibraryCardItem(
    int Id,
    string Type,
    string AssetKey,
    string IconKey,
    string Name,
    string Text,
    GameVersion Version);
