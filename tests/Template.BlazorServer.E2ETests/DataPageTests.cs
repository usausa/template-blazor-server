namespace Template.BlazorServer;

using Microsoft.Playwright;

public sealed class DataPageTests : E2ETestBase
{
    [Fact]
    public async Task ChangeIsShownOnOtherPage()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        var other = await Context.NewPageAsync();
        await other.GotoAsync(factory.ServerAddress + "/data");
        await Page.GotoAsync(factory.ServerAddress + "/data");

        // When
        await AddAsync(Page, "E2E Item", "10");

        // Then
        await Expect(FindRow(other, "E2E Item")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task StaleEditShowsVersionMismatch()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        var other = await Context.NewPageAsync();
        await other.GotoAsync(factory.ServerAddress + "/data");
        await Page.GotoAsync(factory.ServerAddress + "/data");
        await AddAsync(Page, "E2E Edit", "1");
        await Expect(FindRow(other, "E2E Edit")).ToBeVisibleAsync();
        await FindRow(other, "E2E Edit").Locator("button").First.ClickAsync();

        await FindRow(Page, "E2E Edit").Locator("button").First.ClickAsync();
        await Page.Locator(".mud-dialog").GetByLabel("値").FillAsync("2");
        await Page.Locator(".mud-dialog").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();
        await Expect(Page.GetByText("更新しました。")).ToBeVisibleAsync();

        // When
        await other.Locator(".mud-dialog").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();

        // Then
        await Expect(other.GetByText("ほかで更新されています。")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SearchAndPageAreRestoredAfterPause()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        for (var i = 1; i <= 11; i++)
        {
            var created = await Page.APIRequest.PostAsync(factory.ServerAddress + "/api/data/", new APIRequestContextOptions { DataObject = new { name = $"Paged {i:D2}", value = i } });
            Assert.True(created.Ok);
        }

        await Page.GotoAsync(factory.ServerAddress + "/data");
        await Expect(FindRow(Page, "Paged 01")).ToBeVisibleAsync();
        await Page.Locator(".mud-table-pagination-actions button").Nth(2).ClickAsync();
        await Expect(FindRow(Page, "Paged 11")).ToBeVisibleAsync();
        await Page.GetByLabel("名前").FillAsync("Paged");
        await Page.GetByLabel("名前").PressAsync("Tab");

        // When
        Assert.True(await Page.EvaluateAsync<bool>("() => Blazor.pauseCircuit()"));
        Assert.True(await Page.EvaluateAsync<bool>("() => Blazor.resumeCircuit()"));

        // Then
        await Expect(FindRow(Page, "Paged 11")).ToBeVisibleAsync();
        await Expect(FindRow(Page, "Paged 01")).ToBeHiddenAsync();
        await Expect(Page.GetByLabel("名前")).ToHaveValueAsync("Paged");
    }

    private static ILocator FindRow(IPage page, string name) =>
        page.Locator("tr", new PageLocatorOptions { HasText = name });

    private async Task AddAsync(IPage page, string name, string value)
    {
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "追加" }).ClickAsync();
        var dialog = page.Locator(".mud-dialog");
        await dialog.GetByLabel("名前").FillAsync(name);
        await dialog.GetByLabel("値").FillAsync(value);
        await dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK" }).ClickAsync();
        await Expect(page.GetByText("追加しました。")).ToBeVisibleAsync();
    }
}
