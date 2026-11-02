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
        await Expect(Page.Locator(".server-card svg")).ToHaveCountAsync(2, new LocatorAssertionsToHaveCountOptions { Timeout = 15000 });
    }
}
