namespace Template.BlazorServer.Components.Pages;

using Bunit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;

using NSubstitute;

using Template.BlazorServer.Host.Application.Circuits;
using Template.BlazorServer.Host.Components.Pages;
using Template.BlazorServer.Host.Infrastructure.Notifications;

public sealed class HomeTests : MudBlazorTestBase
{
    private readonly NotificationBus bus = new(NullLogger<NotificationBus>.Instance);

    private readonly CircuitTracker tracker = new();

    public HomeTests()
    {
        Services.AddSingleton(bus);
        Services.AddSingleton(tracker);
        Services.AddSingleton(Substitute.For<IFeatureManager>());
        AddAuthorization().SetAuthorized("admin");
    }

    [Fact]
    public void PublishShowsNotification()
    {
        // Arrange
        var cut = Render<Home>();

        // Act
        bus.Publish("Server time: 12:00:00");

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Server time: 12:00:00", cut.Find(".notification-alert").TextContent, StringComparison.Ordinal));
    }

    [Fact]
    public void CircuitChangeUpdatesCount()
    {
        // Arrange
        var cut = Render<Home>();

        // Act
        tracker.Add(new CircuitInfo("a", "admin", DateTimeOffset.UnixEpoch, true));

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("接続中の回線: 1", cut.Markup, StringComparison.Ordinal));
    }
}
