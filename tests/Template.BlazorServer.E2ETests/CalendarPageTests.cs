namespace Template.BlazorServer;

using System.Globalization;

using Microsoft.Playwright;

public sealed class CalendarPageTests : E2ETestBase
{
    private const int HourHeight = 48;

    [Fact]
    public async Task QuickCreateIsShownOnOtherPage()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        var other = await Context.NewPageAsync();
        await OpenAsync(other, factory.ServerAddress);
        await OpenAsync(Page, factory.ServerAddress);

        // When
        await CreateAsync(Page, 3, 10, "E2E 会議");

        // Then
        await Expect(FindEvent(other, "E2E 会議")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task EventCanBeMovedEditedAndDeleted()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await OpenAsync(Page, factory.ServerAddress);
        await CreateAsync(Page, 1, 9, "E2E 移動");

        // When
        var target = Page.Locator(".mc-column").Nth(2);
        var date = await target.GetAttributeAsync("data-date");
        var box = (await FindEvent(Page, "E2E 移動").BoundingBoxAsync())!;
        var column = (await target.BoundingBoxAsync())!;
        await Page.Mouse.MoveAsync(box.X + (box.Width / 2), box.Y + 10);
        await Page.Mouse.DownAsync();
        await Page.Mouse.MoveAsync(box.X + (box.Width / 2), box.Y + 20, new MouseMoveOptions { Steps = 3 });
        await Page.Mouse.MoveAsync(column.X + (column.Width / 2), box.Y + 10 + HourHeight, new MouseMoveOptions { Steps = 12 });
        await Page.Mouse.UpAsync();

        // Then
        await Expect(Page.Locator($".mc-event[data-start='{date}T10:00:00']")).ToHaveCountAsync(1);

        // When
        await FindEvent(Page, "E2E 移動").ClickAsync();
        await Page.Locator(".calendar-popup").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "編集" }).ClickAsync();
        var dialog = Page.Locator(".mud-dialog");
        await dialog.GetByLabel("タイトル").FillAsync("E2E 変更後");
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("更新しました。")).ToBeVisibleAsync();
        await Expect(FindEvent(Page, "E2E 変更後")).ToBeVisibleAsync();

        // When
        await FindEvent(Page, "E2E 変更後").ClickAsync();
        await Page.Locator(".calendar-popup").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "削除" }).ClickAsync();
        await Page.Locator(".mud-dialog").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("削除しました。")).ToBeVisibleAsync();
        await Expect(FindEvent(Page, "E2E 変更後")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ViewAndHiddenCalendarAreRemembered()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await OpenAsync(Page, factory.ServerAddress);
        await CreateAsync(Page, 3, 11, "E2E 隠す");

        // When
        await Page.Keyboard.PressAsync("m");
        await Expect(Page.Locator(".mc-month")).ToBeVisibleAsync();
        await FindCalendar(Page, "仕事").UncheckAsync();
        await Expect(Page.Locator(".mc-bar", new PageLocatorOptions { HasText = "E2E 隠す" })).ToHaveCountAsync(0);
        await Page.ReloadAsync();

        // Then
        await Expect(Page.Locator(".mc-month")).ToBeVisibleAsync();
        await Expect(FindCalendar(Page, "仕事")).Not.ToBeCheckedAsync();
        await Expect(Page.Locator(".mc-bar", new PageLocatorOptions { HasText = "E2E 隠す" })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task CalendarCanBeAddedEditedAndDeleted()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await OpenAsync(Page, factory.ServerAddress);
        var dialog = Page.Locator(".mud-dialog");

        // When
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "カレンダーを追加" }).ClickAsync();
        await dialog.GetByLabel("名前").FillAsync("E2E 予定表");
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "紫" }).ClickAsync();
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("追加しました。")).ToBeVisibleAsync();
        await Expect(FindCalendar(Page, "E2E 予定表")).ToBeCheckedAsync();
        await Expect(FindCalendar(Page, "E2E 予定表")).ToHaveAttributeAsync("style", "accent-color: #A352CC");

        // When
        await OpenCalendarMenuAsync("E2E 予定表", "編集");
        await dialog.GetByLabel("名前").FillAsync("E2E 変更");
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("更新しました。")).ToBeVisibleAsync();
        await Expect(FindCalendar(Page, "E2E 変更")).ToBeVisibleAsync();

        // When
        await OpenCalendarMenuAsync("E2E 変更", "削除");
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("削除しました。")).ToBeVisibleAsync();
        await Expect(FindCalendar(Page, "E2E 変更")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ShortcutsAndAllDayDialogWork()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await OpenAsync(Page, factory.ServerAddress);
        var dialog = Page.Locator(".mud-dialog");

        // When
        await Page.Keyboard.PressAsync("c");

        // Then
        await Expect(dialog.GetByText("予定の作成")).ToBeVisibleAsync();

        // When
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "CANCEL" }).ClickAsync();
        var column = Page.Locator(".mc-column").Nth(3);
        var date = await column.GetAttributeAsync("data-date");
        var box = (await column.BoundingBoxAsync())!;
        await Page.Mouse.ClickAsync(box.X + (box.Width / 2), box.Y + (10 * HourHeight) + 10);
        await Page.GetByLabel("タイトルを追加").FillAsync("E2E 終日");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "その他の設定" }).ClickAsync();
        await dialog.GetByLabel("終日").CheckAsync();
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("予定を作成しました。")).ToBeVisibleAsync();
        await Expect(Page.Locator(".mc-allday .mc-bar", new PageLocatorOptions { HasText = "E2E 終日" })).ToHaveAttributeAsync("data-first", date!);

        // When
        await Page.Keyboard.PressAsync("d");
        await Expect(Page.Locator(".mc-column")).ToHaveCountAsync(1);
        var today = DateOnly.Parse((await Page.Locator(".mc-column").GetAttributeAsync("data-date"))!, CultureInfo.InvariantCulture);
        await Page.Keyboard.PressAsync("j");
        await Expect(Page.Locator(".mc-column")).ToHaveAttributeAsync("data-date", today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        await Page.Keyboard.PressAsync("t");
        await Expect(Page.Locator(".mc-column")).ToHaveAttributeAsync("data-date", today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        await Page.Keyboard.PressAsync("x");

        // Then
        await Expect(Page.Locator(".mc-column")).ToHaveCountAsync(4);
    }

    private static ILocator FindEvent(IPage page, string title) =>
        page.Locator(".mc-event", new PageLocatorOptions { HasText = title });

    private static ILocator FindCalendar(IPage page, string name) =>
        page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = name, Exact = true });

    private async Task OpenCalendarMenuAsync(string name, string action)
    {
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = $"{name} の操作", Exact = true }).ClickAsync();
        await Page.Locator(".mud-popover-open .mud-menu-item", new PageLocatorOptions { HasText = action }).ClickAsync();
    }

    private async Task OpenAsync(IPage page, string serverAddress)
    {
        await page.GotoAsync(serverAddress + "/calendar");
        await Expect(page.Locator(".mc-column").First).ToBeVisibleAsync();
    }

    private async Task CreateAsync(IPage page, int column, int hour, string title)
    {
        var box = (await page.Locator(".mc-column").Nth(column).BoundingBoxAsync())!;
        await page.Mouse.ClickAsync(box.X + (box.Width / 2), box.Y + (hour * HourHeight) + 10);
        var field = page.GetByLabel("タイトルを追加");
        await field.FillAsync(title);
        await field.PressAsync("Enter");
        await Expect(page.GetByText("予定を作成しました。")).ToBeVisibleAsync();
        await Expect(FindEvent(page, title)).ToBeVisibleAsync();
    }
}
