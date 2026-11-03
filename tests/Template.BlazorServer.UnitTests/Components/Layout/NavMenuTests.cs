namespace Template.BlazorServer.Components.Layout;

using Bunit;

using Template.BlazorServer.Host.Application;
using Template.BlazorServer.Host.Components.Layout;

public sealed class NavMenuTests : MudBlazorTestBase
{
    [Fact]
    public void RenderShowsNavigationLinks()
    {
        // Arrange
        AddAuthorization().SetAuthorized("user");

        // Act
        var cut = Render<NavMenu>();

        // Assert
        Assert.Equal(32, cut.FindAll("a").Count);
        Assert.Empty(cut.FindAll("a[href='server']"));
    }

    [Fact]
    public void RenderShowsServerLinkForAdministrator()
    {
        // Arrange
        var authorization = AddAuthorization();
        authorization.SetAuthorized("admin");
        authorization.SetRoles(Roles.Administrator);

        // Act
        var cut = Render<NavMenu>();

        // Assert
        Assert.Equal(33, cut.FindAll("a").Count);
        Assert.Single(cut.FindAll("a[href='server']"));
    }
}
