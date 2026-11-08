namespace Template.BlazorServer.Host.Components.Pages.Components;

internal static class SortableSample
{
    public static readonly string[] Columns = ["To Do", "Doing", "Done"];

    private static readonly string[] Names = ["Apple", "Banana", "Cherry", "Grape", "Lemon", "Orange"];

    public static List<SortItem> CreateItems() =>
        [.. Names.Select(static x => new SortItem(x))];

    public static List<SortTile> CreateTiles() =>
    [
        new("Draft", false),
        new("Pinned", true),
        new("Review", false),
        new("Publish", false)
    ];

    public static Dictionary<string, List<KanbanCard>> CreateColumns() =>
        new()
        {
            ["To Do"] = [new("Design"), new("Review"), new("Document")],
            ["Doing"] = [new("Build"), new("Test")],
            ["Done"] = [new("Plan")]
        };

    public static void Move<T>(List<T> from, int oldIndex, List<T> to, int newIndex)
    {
        var item = from[oldIndex];
        from.RemoveAt(oldIndex);
        to.Insert(newIndex, item);
    }
}

internal sealed class SortItem(string name)
{
    public string Name { get; } = name;
}

internal sealed class SortTile(string name, bool pinned)
{
    public string Name { get; } = name;

    public bool Pinned { get; } = pinned;
}

internal sealed class KanbanCard(string title)
{
    public string Title { get; } = title;
}
