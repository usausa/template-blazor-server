namespace Template.BlazorServer;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

public sealed class NotificationPageTests : E2ETestBase
{
    [Fact]
    public async Task SentNotificationIsShownAndRead()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        var other = await Context.NewPageAsync();
        await other.GotoAsync(factory.ServerAddress + "/");
        await OpenMenuAsync("Features", "notifications");
        await Expect(Page).ToHaveTitleAsync("Notifications");

        // When
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "送信" }).ClickAsync();
        var dialog = Page.Locator(".mud-dialog");
        await dialog.GetByLabel("題").FillAsync("E2E 通知");
        await dialog.GetByLabel("本文").FillAsync("E2E の本文");
        await dialog.GetByLabel("リンク").FillAsync("data");
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("1 人に送りました。")).ToBeVisibleAsync();
        await Expect(other.GetByText("新しい通知: E2E 通知")).ToBeVisibleAsync();
        await Expect(Page.Locator(".notification-bell .mud-badge")).ToHaveTextAsync("1");
        await Expect(Page.Locator(".notification-grid-row.unread", new PageLocatorOptions { HasText = "E2E 通知" })).ToBeVisibleAsync();

        await Page.Locator(".notification-bell").ClickAsync();
        await Page.Locator(".notification-item", new PageLocatorOptions { HasText = "E2E 通知" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/data$"));
        await Expect(Page.Locator(".notification-bell .mud-badge")).ToBeHiddenAsync();
    }
}
