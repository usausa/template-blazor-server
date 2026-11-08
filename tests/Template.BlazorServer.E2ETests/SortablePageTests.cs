namespace Template.BlazorServer;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

public sealed partial class SortablePageTests : E2ETestBase
{
    private const string InitialItems = "Apple, Banana, Cherry, Grape, Lemon, Orange";

    [GeneratedRegex("Done: .*Design")]
    private static partial Regex DesignIsDone();

    [Fact]
    public async Task SortableZoneReordersList()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.GotoAsync(factory.ServerAddress + "/components/sortable");
        var card = Page.Locator(".sample-card", new PageLocatorOptions { HasText = "SortableZone" });
        await Expect(card.Locator(".sample-result")).ToHaveTextAsync(InitialItems);

        // When
        await FindItem(card, ".sample-sort-item", "Apple").DragToAsync(FindItem(card, ".sample-sort-item", "Cherry"));

        // Then
        await ExpectReorderedAsync(card);
    }

    [Fact]
    public async Task SortableZoneMovesCardToAnotherColumn()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);
        await Page.GotoAsync(factory.ServerAddress + "/components/kanban");
        var card = Page.Locator(".sample-card");
        var done = card.Locator(".sample-kanban-column", new LocatorLocatorOptions { HasText = "Done" }).Locator(".sample-kanban-zone");
        await Expect(done).ToBeVisibleAsync();
        await card.ScrollIntoViewIfNeededAsync();

        // When
        await FindItem(card, ".sample-kanban-card", "Design").DragToAsync(done);

        // Then
        await Expect(card.Locator(".sample-result")).ToHaveTextAsync(DesignIsDone());
        await Expect(FindItem(done, ".sample-kanban-card", "Design")).ToBeVisibleAsync();
    }

    private static ILocator FindItem(ILocator parent, string selector, string text) =>
        parent.Locator(selector, new LocatorLocatorOptions { HasText = text });

    private async Task ExpectReorderedAsync(ILocator card)
    {
        var result = card.Locator(".sample-result");
        await Expect(result).Not.ToHaveTextAsync(InitialItems);
        var names = await card.Locator(".sample-sort-item").AllInnerTextsAsync();
        await Expect(result).ToHaveTextAsync(String.Join(", ", names.Select(static x => x.Trim())));
    }
}
