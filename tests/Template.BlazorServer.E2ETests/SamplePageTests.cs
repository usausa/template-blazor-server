namespace Template.BlazorServer;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

public sealed class SamplePageTests : E2ETestBase
{
    private static readonly (string Path, string Title, string Selector)[] SamplePages =
    [
        ("smart/rendering", "Rendering - Smart.Blazor", ".sample-loading"),
        ("smart/forms", "Forms - Smart.Blazor", ".sample-form"),
        ("components/keyboard", "Keyboard - Components.Blazor", ".sample-keys"),
        ("components/display", "Display - Components.Blazor", "dialog.sample-dialog"),
        ("components/script", "Script - Components.Blazor", ".sample-measure"),
        ("components/device", "Device - Components.Blazor", ".sample-card"),
        ("components/speaker", "Speaker - Components.Blazor", ".mud-slider"),
        ("components/tabs", "Tabs - Components.Blazor", ".sample-card"),
        ("components/upload", "Upload - Components.Blazor", ".sample-upload"),
        ("musselchart/monitor", "Monitor - MusselChart", ".mussel-stat"),
        ("musselchart/charts", "Charts - MusselChart", ".mussel-chart-svg"),
        ("musselchart/widgets", "Widgets - MusselChart", ".mussel-sparkline"),
        ("textresource/locale", "Locale - TextResource", ".sample-card"),
        ("ignite/buttons", "Buttons - Ignite UI", "igc-button"),
        ("ignite/inputs", "Inputs - Ignite UI", "igc-input"),
        ("ignite/selection", "Selection - Ignite UI", "igc-combo"),
        ("ignite/scheduling", "Scheduling - Ignite UI", "igc-calendar"),
        ("ignite/layout", "Layout - Ignite UI", "igc-tile-manager"),
        ("ignite/navigation", "Navigation - Ignite UI", "igc-stepper"),
        ("ignite/display", "Display - Ignite UI", "igc-qr-code"),
        ("ignite/notifications", "Notifications - Ignite UI", "igc-dialog"),
        ("ignite/chat", "Chat - Ignite UI", "igc-chat"),
        ("ignite/grid", "Grid Lite - Ignite UI", "igc-grid-lite"),
        ("syncfusion/buttons", "Buttons - Syncfusion", ".e-btn-group"),
        ("syncfusion/inputs", "Inputs - Syncfusion", ".e-upload"),
        ("syncfusion/calendars", "Calendars - Syncfusion", ".e-calendar"),
        ("syncfusion/popups", "Popups - Syncfusion", ".e-spinner-pane"),
        ("syncfusion/chart", "Chart - Syncfusion", ".e-chart")
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

        foreach (var (path, title, selector) in SamplePages)
        {
            // When
            await Page.Locator($"a[href='{path}']").ClickAsync();

            // Then
            await Expect(Page).ToHaveTitleAsync(title);
            await Expect(Page.Locator(selector).First).ToBeAttachedAsync();
        }

        Assert.Empty(errors);
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
        await Page.Locator("a[href='ignite/grid']").ClickAsync();
        await Expect(Page.Locator("igc-grid-lite")).ToBeVisibleAsync();

        // When
        await Page.Locator("a[href='ignite/selection']").ClickAsync();
        await Page.Locator("igc-combo").ClickAsync().WaitAsync(cancellationToken);
        await Page.GetByText("Osaka", new PageGetByTextOptions { Exact = true }).ClickAsync().WaitAsync(cancellationToken);

        // Then
        await Expect(Page.GetByText(new Regex("^Value: .*osaka"))).ToBeVisibleAsync();
    }
}
