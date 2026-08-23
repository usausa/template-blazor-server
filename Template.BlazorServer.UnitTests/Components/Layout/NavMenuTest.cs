namespace Template.BlazorServer.Components.Layout;

using Bunit;

using Template.BlazorServer.Host.Components.Layout;

public sealed class NavMenuTest : MudBlazorTestBase
{
    [Fact]
    public void RenderShowsNavigationLinks()
    {
        // Arrange & Act
        var cut = Render<NavMenu>();

        // Assert
        var links = cut.FindAll("a");
        Assert.Equal(3, links.Count);
    }
}
