namespace Template.BlazorServer.Components.Pages.TextResource;

using System.Globalization;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

using Template.BlazorServer.Host.Components.Pages.TextResource;
using Template.BlazorServer.Host.Resources;

public sealed class LocalePageTests : MudBlazorTestBase
{
    public LocalePageTests()
    {
        AddAuthorization().SetAuthorized("admin");
    }

    [Fact]
    public void KeysAreListedWithEachLanguage()
    {
        // Act
        var cut = Render<LocalePage>();

        // Assert
        var rows = cut.FindAll(".sample-wide tbody tr");
        Assert.Equal(Messages.Keys.Count, rows.Count);
        Assert.Equal(
            ["HelloWorld", "ハローワールド", "Hello World"],
            rows[0].QuerySelectorAll("td").Take(3).Select(static x => x.TextContent));
    }

    [Theory]
    [InlineData("ja-JP", "ハローワールド", "合計 1,980 円")]
    [InlineData("en-US", "Hello World", "Total 1,980 yen")]
    public void MembersFollowUiCulture(string name, string hello, string total)
    {
        // Arrange
        var original = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
        try
        {
            // Act
            var cut = Render<LocalePage>();

            // Assert
            var members = cut.FindAll(".sample-card").Single(static x => x.TextContent.Contains("Members", StringComparison.Ordinal)).TextContent;
            Assert.Contains(hello, members, StringComparison.Ordinal);
            Assert.Contains(total, members, StringComparison.Ordinal);
            Assert.Contains($"CurrentUICulture: {name}", cut.Find(".sample-result").TextContent, StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    [Fact]
    public void LanguageButtonReloadsThroughCultureEndpoint()
    {
        // Arrange
        var cut = Render<LocalePage>();
        var navigation = Services.GetRequiredService<BunitNavigationManager>();

        // Act
        cut.FindAll("button").First(static x => x.TextContent.Contains("English", StringComparison.Ordinal)).Click();

        // Assert
        cut.WaitForAssertion(() => Assert.EndsWith("culture/en-US", navigation.Uri, StringComparison.Ordinal));
        Assert.True(navigation.History.First().Options.ForceLoad);
    }
}
