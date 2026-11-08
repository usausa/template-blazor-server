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
        await Expect(Page).ToHaveTitleAsync("Validation - Smart.Blazor");

        // When
        await Page.GetByLabel("Name").FillAsync("Taken");
        await Page.GetByLabel("Email").FillAsync("taken@example.com");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("This name is used in Data.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Check the fields below.")).ToBeVisibleAsync();
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
    public async Task TextIsDownloadedWithoutSavePicker()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.AddInitScriptAsync("delete window.showSaveFilePicker;");
        await Page.GotoAsync(factory.ServerAddress + "/components/save");
        await Expect(Page).ToHaveTitleAsync("Save - Components.Blazor");

        // When
        var download = await Page.RunAndWaitForDownloadAsync(() => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save text" }).ClickAsync());

        // Then
        Assert.Equal("sample.txt", download.SuggestedFilename);
        await Expect(Page.GetByText("Downloaded")).ToBeVisibleAsync();
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

    [Fact]
    public async Task LanguageIsChangedOnLocalePage()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.GotoAsync(factory.ServerAddress + "/textresource/locale");
        await Expect(Page).ToHaveTitleAsync("Locale - TextResource");
        var members = Page.Locator(".sample-card", new PageLocatorOptions { HasText = "Members" });
        var validation = Page.Locator(".sample-card", new PageLocatorOptions { HasText = "WithMessage" });
        var validate = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Validate" });

        await Expect(Page.GetByText("CurrentCulture: ja-JP / CurrentUICulture: ja-JP")).ToBeVisibleAsync();
        await Expect(members.GetByText("ハローワールド")).ToBeVisibleAsync();
        await Expect(members.GetByText("合計 1,980 円")).ToBeVisibleAsync();
        await validate.ClickAsync();
        await Expect(validation.GetByText("名前を入力してください。")).ToBeVisibleAsync();

        // When
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "English" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("CurrentCulture: en-US / CurrentUICulture: en-US")).ToBeVisibleAsync();
        await Expect(members.GetByText("Hello World")).ToBeVisibleAsync();
        await Expect(members.GetByText("Total 1,980 yen")).ToBeVisibleAsync();
        await validate.ClickAsync();
        await Expect(validation.GetByText("Please enter a name.")).ToBeVisibleAsync();

        // When
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "日本語" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("CurrentCulture: ja-JP / CurrentUICulture: ja-JP")).ToBeVisibleAsync();
        await Expect(members.GetByText("ハローワールド")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task BrowserLanguageDoesNotChangeCulture()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await using var context = await Browser.NewContextAsync(new BrowserNewContextOptions { Locale = "en-US" });
        var page = await context.NewPageAsync();
        await LoginAsync(page, factory.ServerAddress);

        // When
        await page.GotoAsync(factory.ServerAddress + "/textresource/locale");

        // Then
        await Expect(page.GetByText("CurrentCulture: ja-JP / CurrentUICulture: ja-JP")).ToBeVisibleAsync();
    }
}
