namespace Template.BlazorServer;

using Microsoft.Playwright;

public sealed class ReportPageTests : E2ETestBase
{
    [Fact]
    public async Task PreviewOpensPdfInNewTab()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.Locator("a[href='reports']").ClickAsync();
        await Expect(Page).ToHaveTitleAsync("Reports");

        var pdfResponse = new TaskCompletionSource<IResponse>();
        Context.Response += (_, response) =>
        {
            if (response.Url.Contains("/api/reports/product-list", StringComparison.Ordinal))
            {
                pdfResponse.TrySetResult(response);
            }
        };

        // When
        await Page.RunAndWaitForPopupAsync(() => Page.Locator(".report-card").Nth(1).GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "プレビュー" }).ClickAsync());
        var response = await pdfResponse.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // Then
        Assert.Contains("/api/reports/product-list?rows=50", response.Url, StringComparison.Ordinal);
        Assert.Equal(200, response.Status);
        Assert.Equal("application/pdf", response.Headers["content-type"]);
        Assert.StartsWith("inline", response.Headers["content-disposition"], StringComparison.Ordinal);
    }

    [Fact]
    public async Task DownloadSavesSelectedSlips()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.Locator("a[href='reports']").ClickAsync();
        var card = Page.Locator(".report-card").Nth(2);
        await card.GetByLabel("納品書").UncheckAsync();

        // When
        var download = await Page.RunAndWaitForDownloadAsync(() => card.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "ダウンロード" }).ClickAsync());

        // Then
        Assert.Equal("slip.pdf", download.SuggestedFilename);
        Assert.Contains("sheets=invoice&sheets=receipt", download.Url, StringComparison.Ordinal);
    }

    private async Task LoginAsync(string serverAddress)
    {
        await Page.GotoAsync(serverAddress + "/login");
        await Page.FillAsync("#name", "admin");
        await Page.FillAsync("#password", "admin");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "ログイン" }).ClickAsync();
        await Expect(Page.GetByText("Template.BlazorServer")).ToBeVisibleAsync();
    }
}
