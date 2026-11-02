namespace Template.BlazorServer;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

public sealed class FilePageTests : E2ETestBase
{
    private const string MakeTransfer = """
        (files) => {
            const transfer = new DataTransfer();
            for (const file of files) {
                const bytes = Uint8Array.from(atob(file.data), c => c.charCodeAt(0));
                transfer.items.add(new File([bytes], file.name, { type: file.type }));
            }
            return transfer;
        }
        """;

    private const string DispatchPaste = """
        (zone, transfer) => zone.dispatchEvent(new ClipboardEvent("paste", { clipboardData: transfer, bubbles: true, cancelable: true }))
        """;

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
        await Page.Locator(".file-inputs input[type=file]").First.SetInputFilesAsync(new FilePayload { Name = "e2e.png", MimeType = "image/png", Buffer = Png });
        await Expect(Page.Locator(".upload-item img.upload-thumb")).ToHaveJSPropertyAsync("naturalWidth", 1);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "アップロード", Exact = true }).ClickAsync();

        // Then
        var row = Page.Locator("tr", new PageLocatorOptions { HasText = "e2e.png" });
        await Expect(row.Locator("img.file-thumb")).ToHaveJSPropertyAsync("naturalWidth", 1);
        await Expect(Page.Locator(".upload-queue")).ToBeHiddenAsync();
        await row.ClickAsync();
        await Expect(Page.Locator(".mud-dialog .file-preview-image")).ToHaveJSPropertyAsync("naturalWidth", 1);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "閉じる" }).ClickAsync();

        await Page.Locator(".files-view button").Nth(1).ClickAsync();
        await Expect(Page.Locator(".file-card", new PageLocatorOptions { HasText = "e2e.png" }).Locator("img")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task PickedDroppedAndPastedFilesAreUploaded()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.GotoAsync(factory.ServerAddress + "/files");
        await Expect(Page).ToHaveTitleAsync("Files");
        var zone = Page.Locator(".files-drop");

        // When
        await Page.Locator(".file-inputs input[type=file]").First.SetInputFilesAsync(
        [
            new FilePayload { Name = "picked.txt", MimeType = "text/plain", Buffer = "picked"u8.ToArray() },
            new FilePayload { Name = "picked.png", MimeType = "image/png", Buffer = Png }
        ]);
        await Expect(Page.Locator(".upload-item")).ToHaveCountAsync(2);

        var dropped = await Page.EvaluateHandleAsync(MakeTransfer, new[] { new { name = "dropped.txt", type = "text/plain", data = Convert.ToBase64String("dropped"u8) } });
        await zone.DispatchEventAsync("dragenter", new { dataTransfer = dropped });
        await Expect(zone).ToHaveClassAsync(new Regex("files-drop-active"));
        await zone.DispatchEventAsync("drop", new { dataTransfer = dropped });
        await Expect(Page.Locator(".upload-item")).ToHaveCountAsync(3);

        for (var i = 0; i < 2; i++)
        {
            var pasted = await Page.EvaluateHandleAsync(MakeTransfer, new[] { new { name = "image.png", type = "image/png", data = Convert.ToBase64String(Png) } });
            await zone.EvaluateAsync(DispatchPaste, pasted);
            await Expect(Page.Locator(".upload-item")).ToHaveCountAsync(4 + i);
        }

        await Expect(Page.Locator(".upload-item-name", new PageLocatorOptions { HasText = "image (2).png" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "アップロード", Exact = true }).ClickAsync();

        // Then
        await Expect(Page.GetByText("5 件をアップロードしました。")).ToBeVisibleAsync();
        await Expect(Page.Locator(".upload-queue")).ToBeHiddenAsync();
        foreach (var name in new[] { "picked.txt", "picked.png", "dropped.txt", "image.png", "image (2).png" })
        {
            await Expect(Page.Locator("tr", new PageLocatorOptions { HasText = name })).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task OverwriteIsConfirmed()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.GotoAsync(factory.ServerAddress + "/files");
        await Expect(Page).ToHaveTitleAsync("Files");
        var input = Page.Locator(".file-inputs input[type=file]").First;
        var upload = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "アップロード", Exact = true });
        await input.SetInputFilesAsync(new FilePayload { Name = "same.txt", MimeType = "text/plain", Buffer = "first"u8.ToArray() });
        await upload.ClickAsync();
        await Expect(Page.Locator("tr", new PageLocatorOptions { HasText = "same.txt" })).ToBeVisibleAsync();

        // When
        await input.SetInputFilesAsync(new FilePayload { Name = "same.txt", MimeType = "text/plain", Buffer = "second"u8.ToArray() });
        await upload.ClickAsync();
        var dialog = Page.Locator(".mud-dialog");
        await Expect(dialog).ToContainTextAsync("same.txt");
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "CANCEL" }).ClickAsync();
        await Expect(Page.Locator(".upload-item")).ToHaveCountAsync(1);

        await upload.ClickAsync();
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();

        // Then
        await Expect(Page.Locator(".upload-queue")).ToBeHiddenAsync();
        var response = await Page.APIRequest.GetAsync(factory.ServerAddress + "/api/files/download/same.txt");
        Assert.Equal("second", await response.TextAsync());
    }
}
