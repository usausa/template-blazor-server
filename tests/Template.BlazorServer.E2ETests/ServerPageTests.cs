namespace Template.BlazorServer;

using Microsoft.Playwright;

public sealed class ServerPageTests : E2ETestBase
{
    [Fact]
    public async Task ServerPageShowsStatus()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);

        // When
        await Page.Locator("a[href='server']").ClickAsync();

        // Then
        await Expect(Page).ToHaveTitleAsync("Server");
        await Expect(Page.Locator(".server-table")).ToContainTextAsync("admin");
        await Expect(Page.Locator(".server-card .mussel-chart-svg")).ToHaveCountAsync(2);
        await Expect(Page.Locator(".server-card polyline").First).ToBeAttachedAsync(new LocatorAssertionsToBeAttachedOptions { Timeout = 15000 });
    }
}
