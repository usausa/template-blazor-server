namespace Template.BlazorServer.Host.Components.Pages.Components;

using Microsoft.AspNetCore.Components;

public sealed partial class KeyboardPage
{
    private const int LogSize = 8;

    private const int LevelStep = 5;

    private static readonly Shortcut[] Shortcuts =
    [
        new("Ctrl+S", "Save (also in the text field)"),
        new("F2", "Edit"),
        new("?", "Question"),
        new("Shift+1", "First (physical key)"),
        new("↑ / ↓", "Level (repeat while held)")
    ];

    private readonly List<string> log = [];

    private bool editDisabled;

    private bool helpVisible;

    private int level = 50;

    private string lastAction = "-";

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required TimeProvider TimeProvider { get; set; }

    private string HelpText => helpVisible ? "shown" : "hidden";

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnSave() => Record("Ctrl+S", "Save");

    private void OnEdit() => Record("F2", "Edit");

    private void OnQuestion() => Record("?", "Question");

    private void OnFirst() => Record("Shift+1", "First");

    private void OnUp() => level = Math.Min(level + LevelStep, 100);

    private void OnDown() => level = Math.Max(level - LevelStep, 0);

    private void ShowHelp() => helpVisible = true;

    private void HideHelp() => helpVisible = false;

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void Record(string key, string action)
    {
        lastAction = action;
        log.Insert(0, $"{TimeProvider.GetLocalNow():HH:mm:ss} {key} → {action}");
        if (log.Count > LogSize)
        {
            log.RemoveAt(log.Count - 1);
        }
    }

    //--------------------------------------------------------------------------------
    // View
    //--------------------------------------------------------------------------------

    private sealed record Shortcut(string Key, string Action);
}
