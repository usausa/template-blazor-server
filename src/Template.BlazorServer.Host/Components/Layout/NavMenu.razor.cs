namespace Template.BlazorServer.Host.Components.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using MudBlazor;

using Template.BlazorServer.Host.Application;

public sealed partial class NavMenu
{
    private static readonly NavGroup[] Groups =
    [
        new("Features", Icons.Material.Filled.Dashboard,
        [
            new("data", "Data"),
            new("files", "Files"),
            new("reports", "Reports"),
            new("notifications", "Notifications"),
            new("server", "Server", Roles.Administrator)
        ]),
        new("Fundamentals", Icons.Material.Filled.Foundation,
        [
            new("smart/rendering", "Rendering"),
            new("components/display", "Display"),
            new("components/script", "Script"),
            new("textresource/locale", "Locale")
        ]),
        new("Forms", Icons.Material.Filled.EditNote,
        [
            new("smart/forms", "Validation")
        ]),
        new("Sheets", Icons.Material.Filled.GridOn,
        [
            new("musselsheet/form", "Form"),
            new("musselsheet/cell-kinds", "Cell kinds"),
            new("musselsheet/object-binding", "Object binding")
        ]),
        new("Collections", Icons.Material.Filled.ViewList,
        [
            new("components/sortable", "Sortable"),
            new("components/kanban", "Kanban")
        ]),
        new("Charts", Icons.Material.Filled.ShowChart,
        [
            new("musselchart/monitor", "Monitor"),
            new("musselchart/charts", "Charts"),
            new("musselchart/bars", "Bars"),
            new("musselchart/points", "Points"),
            new("musselchart/pie", "Pie"),
            new("musselchart/radar", "Radar"),
            new("musselchart/heatmap", "Heatmap"),
            new("musselchart/timeline", "State timeline"),
            new("musselchart/radial", "Radial bars"),
            new("musselchart/widgets", "Widgets")
        ]),
        new("Platform", Icons.Material.Filled.Devices,
        [
            new("components/keyboard", "Keyboard"),
            new("components/device", "Device"),
            new("components/speaker", "Speaker"),
            new("components/tabs", "Tabs"),
            new("components/upload", "Upload"),
            new("components/save", "Save")
        ]),
        new("Ignite UI", Icons.Material.Filled.Widgets,
        [
            new("ignite/buttons", "Buttons"),
            new("ignite/inputs", "Inputs"),
            new("ignite/selection", "Selection"),
            new("ignite/scheduling", "Scheduling"),
            new("ignite/layout", "Layout"),
            new("ignite/navigation", "Navigation"),
            new("ignite/display", "Display"),
            new("ignite/notifications", "Notifications"),
            new("ignite/chat", "Chat"),
            new("ignite/grid", "Grid Lite")
        ]),
        new("Syncfusion", Icons.Material.Filled.Extension,
        [
            new("syncfusion/buttons", "Buttons"),
            new("syncfusion/inputs", "Inputs"),
            new("syncfusion/calendars", "Calendars"),
            new("syncfusion/popups", "Popups"),
            new("syncfusion/chart", "Chart")
        ])
    ];

    private readonly HashSet<string> expanded = [];

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required NavigationManager Navigation { get; set; }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        ExpandCurrent();
        Navigation.LocationChanged += OnLocationChanged;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Navigation.LocationChanged -= OnLocationChanged;
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (ExpandCurrent())
        {
            _ = InvokeAsync(StateHasChanged);
        }
    }

    private void OnExpandedChanged(NavGroup group, bool value)
    {
        if (value)
        {
            expanded.Add(group.Title);
        }
        else
        {
            expanded.Remove(group.Title);
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private bool IsExpanded(NavGroup group) => expanded.Contains(group.Title);

    private bool ExpandCurrent()
    {
        var path = Navigation.ToBaseRelativePath(Navigation.Uri);
        var end = path.IndexOfAny(['?', '#']);
        if (end >= 0)
        {
            path = path[..end];
        }

        var group = Array.Find(Groups, x => x.Items.Any(item => item.Href == path));
        return (group is not null) && expanded.Add(group.Title);
    }

    private sealed record NavGroup(string Title, string Icon, NavItem[] Items);

    private sealed record NavItem(string Href, string Text, string? Role = null);
}
