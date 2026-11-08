namespace Template.BlazorServer.Host.Components.Pages.Components;

using BlazorComponents;

public sealed partial class SortablePage
{
    private readonly List<SortItem> items = SortableSample.CreateItems();

    private readonly List<SortTile> tiles = SortableSample.CreateTiles();

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    private string ItemsResult => String.Join(", ", items.Select(static x => x.Name));

    private string TilesResult => String.Join(", ", tiles.Select(static x => x.Name));

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnItemMoved(SortableMoveEventArgs e) =>
        SortableSample.Move(items, e.OldIndex, items, e.NewIndex);

    private void OnTileMoved(SortableMoveEventArgs e) =>
        SortableSample.Move(tiles, e.OldIndex, tiles, e.NewIndex);
}
