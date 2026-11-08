namespace Template.BlazorServer;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

public sealed class SamplePageTests : E2ETestBase
{
    private static readonly (string Group, string Path, string Title, string Selector)[] SamplePages =
    [
        ("Fundamentals", "smart/rendering", "Rendering - Smart.Blazor", ".sample-loading"),
        ("Fundamentals", "components/display", "Display - Components.Blazor", "dialog.sample-dialog"),
        ("Fundamentals", "components/script", "Script - Components.Blazor", ".sample-measure"),
        ("Fundamentals", "textresource/locale", "Locale - TextResource", ".sample-card"),
        ("Forms", "smart/forms", "Validation - Smart.Blazor", ".sample-form"),
        ("Sheets", "musselsheet/form", "Form - MusselSheet", ".mussel-sheet-viewport"),
        ("Sheets", "musselsheet/cell-kinds", "Cell kinds - MusselSheet", ".mussel-sheet-viewport"),
        ("Sheets", "musselsheet/object-binding", "Object binding - MusselSheet", ".mussel-sheet-viewport"),
        ("Collections", "components/sortable", "Sortable - Components.Blazor", ".sample-sort-item"),
        ("Collections", "components/kanban", "Kanban - Components.Blazor", ".sample-kanban-card"),
        ("Charts", "musselchart/monitor", "Monitor - MusselChart", ".mussel-stat"),
        ("Charts", "musselchart/charts", "Charts - MusselChart", ".mussel-chart-svg"),
        ("Charts", "musselchart/bars", "Bars - MusselChart", ".mussel-chart-svg"),
        ("Charts", "musselchart/points", "Points - MusselChart", ".mussel-chart-svg"),
        ("Charts", "musselchart/pie", "Pie - MusselChart", ".mussel-pie"),
        ("Charts", "musselchart/radar", "Radar - MusselChart", ".mussel-radar"),
        ("Charts", "musselchart/heatmap", "Heatmap - MusselChart", ".mussel-heatmap"),
        ("Charts", "musselchart/timeline", "State timeline - MusselChart", ".mussel-timeline"),
        ("Charts", "musselchart/radial", "Radial bars - MusselChart", ".mussel-radial"),
        ("Charts", "musselchart/widgets", "Widgets - MusselChart", ".mussel-sparkline"),
        ("Platform", "components/keyboard", "Keyboard - Components.Blazor", ".sample-keys"),
        ("Platform", "components/device", "Device - Components.Blazor", ".sample-card"),
        ("Platform", "components/speaker", "Speaker - Components.Blazor", ".mud-slider"),
        ("Platform", "components/tabs", "Tabs - Components.Blazor", ".sample-card"),
        ("Platform", "components/upload", "Upload - Components.Blazor", ".sample-upload"),
        ("Platform", "components/save", "Save - Components.Blazor", ".sample-card"),
        ("Ignite UI", "ignite/buttons", "Buttons - Ignite UI", "igc-button"),
        ("Ignite UI", "ignite/inputs", "Inputs - Ignite UI", "igc-input"),
        ("Ignite UI", "ignite/selection", "Selection - Ignite UI", "igc-combo"),
        ("Ignite UI", "ignite/scheduling", "Scheduling - Ignite UI", "igc-calendar"),
        ("Ignite UI", "ignite/layout", "Layout - Ignite UI", "igc-tile-manager"),
        ("Ignite UI", "ignite/navigation", "Navigation - Ignite UI", "igc-stepper"),
        ("Ignite UI", "ignite/display", "Display - Ignite UI", "igc-qr-code"),
        ("Ignite UI", "ignite/notifications", "Notifications - Ignite UI", "igc-dialog"),
        ("Ignite UI", "ignite/chat", "Chat - Ignite UI", "igc-chat"),
        ("Ignite UI", "ignite/grid", "Grid Lite - Ignite UI", "igc-grid-lite"),
        ("Syncfusion", "syncfusion/buttons", "Buttons - Syncfusion", ".e-btn-group"),
        ("Syncfusion", "syncfusion/inputs", "Inputs - Syncfusion", ".e-upload"),
        ("Syncfusion", "syncfusion/calendars", "Calendars - Syncfusion", ".e-calendar"),
        ("Syncfusion", "syncfusion/popups", "Popups - Syncfusion", ".e-spinner-pane"),
        ("Syncfusion", "syncfusion/chart", "Chart - Syncfusion", ".e-chart")
    ];

    [Fact]
    public async Task SamplePagesShowComponentsWithoutErrors()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        var errors = new List<string>();
        Page.Console += (_, message) =>
        {
            if (message.Type == "error")
            {
                errors.Add(message.Text);
            }
        };
        Page.PageError += (_, error) => errors.Add(error);

        await LoginAsync(factory.ServerAddress);

        foreach (var (group, path, title, selector) in SamplePages)
        {
            // When
            await OpenMenuAsync(group, path);

            // Then
            await Expect(Page).ToHaveTitleAsync(title);
            await Expect(Page.Locator(selector).First).ToBeAttachedAsync();
        }

        Assert.Empty(errors);
    }

    [Fact]
    public async Task MenuGroupsAreClosedExceptCurrent()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Expect(Page.Locator("a[href='data']")).Not.ToBeVisibleAsync();

        // When
        await Page.GotoAsync(factory.ServerAddress + "/components/kanban");

        // Then
        await Expect(Page.Locator("a[href='components/kanban']")).ToBeVisibleAsync();
        await Expect(Page.Locator("a[href='data']")).Not.ToBeVisibleAsync();
    }

    [Fact(Timeout = 120_000)]
    public async Task ComboWorksAfterGridLite()
    {
        // Given
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await OpenMenuAsync("Ignite UI", "ignite/grid");
        await Expect(Page.Locator("igc-grid-lite")).ToBeVisibleAsync();

        // When
        await OpenMenuAsync("Ignite UI", "ignite/selection");
        await Page.Locator("igc-combo").ClickAsync().WaitAsync(cancellationToken);
        await Page.GetByText("Osaka", new PageGetByTextOptions { Exact = true }).ClickAsync().WaitAsync(cancellationToken);

        // Then
        await Expect(Page.GetByText(new Regex("^Value: .*osaka"))).ToBeVisibleAsync();
    }
}
