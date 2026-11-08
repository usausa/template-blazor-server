namespace Template.BlazorServer.Host.Components.Pages.Components;

using BlazorComponents;

public sealed partial class KanbanPage
{
    private readonly Dictionary<string, List<KanbanCard>> columns = SortableSample.CreateColumns();

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    private string CardsResult =>
        String.Join(" / ", SortableSample.Columns.Select(column => $"{column}: {String.Join(", ", columns[column].Select(static x => x.Title))}"));

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnCardMoved(SortableMoveEventArgs e) =>
        SortableSample.Move(columns[e.From], e.OldIndex, columns[e.To], e.NewIndex);
}
