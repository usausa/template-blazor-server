namespace Template.BlazorServer;

using Microsoft.Playwright;

public sealed class LibrarySampleTests : E2ETestBase
{
    private const int Retry = 20;

    [Fact]
    public async Task ServerErrorIsShownOnField()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        var created = await Page.APIRequest.PostAsync(factory.ServerAddress + "/api/data/", new APIRequestContextOptions { DataObject = new { name = "Taken", value = 1 } });
        Assert.True(created.Ok);
        await Page.GotoAsync(factory.ServerAddress + "/smart/forms");
        await Expect(Page).ToHaveTitleAsync("Forms - Smart.Blazor");

        // When
        await Page.GetByLabel("Name").FillAsync("Taken");
        await Page.GetByLabel("Email").FillAsync("taken@example.com");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("This name is used in Data.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Check the fields below.")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task FormInputIsRestoredAfterPause()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.GotoAsync(factory.ServerAddress + "/smart/forms");
        await Expect(Page).ToHaveTitleAsync("Forms - Smart.Blazor");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Fill by code" }).ClickAsync();
        await Expect(Page.GetByText("Filled by code")).ToBeVisibleAsync();
        await Page.GetByLabel("Name").FillAsync("Restored");
        await Page.GetByLabel("Name").PressAsync("Tab");

        // When
        Assert.True(await Page.EvaluateAsync<bool>("() => Blazor.pauseCircuit()"));
        Assert.True(await Page.EvaluateAsync<bool>("() => Blazor.resumeCircuit()"));

        // Then
        await Expect(Page.GetByLabel("Name")).ToHaveValueAsync("Restored");
        await Expect(Page.GetByLabel("Email")).ToHaveValueAsync("sample");
        await Expect(Page.GetByText("Filled by code")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task HelpIsShownWithF1()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.GotoAsync(factory.ServerAddress + "/components/keyboard");
        await Expect(Page).ToHaveTitleAsync("Keyboard - Components.Blazor");
        var help = Page.Locator(".sample-help");

        // When
        for (var i = 0; (i < Retry) && !await help.IsVisibleAsync(); i++)
        {
            await Page.Keyboard.PressAsync("F1");
            await Page.WaitForTimeoutAsync(250);
        }

        // Then
        await Expect(help).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(help).ToBeHiddenAsync();

        await Page.GetByLabel("Ctrl+S works while typing here").FillAsync("memo");
        await Page.Keyboard.PressAsync("Control+s");
        await Expect(Page.Locator(".sample-log").First).ToContainTextAsync("Ctrl+S → Save");
    }

    [Fact]
    public async Task MessageIsSentToOtherTab()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        var other = await Context.NewPageAsync();
        await other.GotoAsync(factory.ServerAddress + "/components/tabs");
        await Expect(other).ToHaveTitleAsync("Tabs - Components.Blazor");
        await Page.GotoAsync(factory.ServerAddress + "/components/tabs");
        await Expect(Page).ToHaveTitleAsync("Tabs - Components.Blazor");
        var received = other.Locator(".sample-log", new PageLocatorOptions { HasText = "E2E message" });

        // When
        await Page.GetByLabel("Message").FillAsync("E2E message");
        for (var i = 0; (i < Retry) && (await received.CountAsync() == 0); i++)
        {
            await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Tell" }).ClickAsync();
            await Page.WaitForTimeoutAsync(250);
        }

        // Then
        await Expect(received.First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task FilesAreSentByFileUpload()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.GotoAsync(factory.ServerAddress + "/components/upload");
        await Expect(Page).ToHaveTitleAsync("Upload - Components.Blazor");

        // When
        var chooser = await Page.RunAndWaitForFileChooserAsync(() => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Files", Exact = true }).ClickAsync());
        await chooser.SetFilesAsync(
        [
            new FilePayload { Name = "first.txt", MimeType = "text/plain", Buffer = "first"u8.ToArray() },
            new FilePayload { Name = "second.txt", MimeType = "text/plain", Buffer = "second"u8.ToArray() }
        ]);

        // Then
        await Expect(Page.GetByText("Completed: 2 succeeded, 0 failed")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task MonitorTooltipsFollowPointer()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.GotoAsync(factory.ServerAddress + "/musselchart/monitor");
        await Expect(Page).ToHaveTitleAsync("Monitor - MusselChart");
        var chart = Page.Locator(".mussel-chart").First;
        await Expect(chart.Locator("polyline").First).ToBeAttachedAsync();
        var box = await chart.BoundingBoxAsync();
        var tooltips = Page.Locator(".mussel-tooltip:not([hidden])");

        // When
        for (var i = 0; (i < Retry) && (await tooltips.CountAsync() < 3); i++)
        {
            await Page.Mouse.MoveAsync(box!.X + (box.Width * 0.7f) + i, box.Y + (box.Height * 0.3f));
            await Page.WaitForTimeoutAsync(250);
        }

        // Then
        await Expect(tooltips).ToHaveCountAsync(3);
    }
}
