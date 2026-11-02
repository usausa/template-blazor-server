namespace Template.BlazorServer;

using Microsoft.Playwright;

public sealed class FilePageTests : E2ETestBase
{
    private static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    [Fact]
    public async Task UploadedImageIsShownAndPreviewed()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.Locator("a[href='files']").ClickAsync();
        await Expect(Page).ToHaveTitleAsync("Files");

        // When
        await Page.Locator("input[type=file]").SetInputFilesAsync(new FilePayload { Name = "e2e.png", MimeType = "image/png", Buffer = Png });

        // Then
        var row = Page.Locator("tr", new PageLocatorOptions { HasText = "e2e.png" });
        await Expect(row.Locator("img.file-thumb")).ToHaveJSPropertyAsync("naturalWidth", 1);
        await row.ClickAsync();
        await Expect(Page.Locator(".mud-dialog .file-preview-image")).ToHaveJSPropertyAsync("naturalWidth", 1);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "閉じる" }).ClickAsync();

        await Page.Locator(".files-view button").Nth(1).ClickAsync();
        await Expect(Page.Locator(".file-card", new PageLocatorOptions { HasText = "e2e.png" }).Locator("img")).ToBeVisibleAsync();
    }
}
