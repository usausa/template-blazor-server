namespace Template.BlazorServer.Infrastructure.Notifications;

using Microsoft.Extensions.Logging.Abstractions;

using Template.BlazorServer.Host.Infrastructure.Notifications;

public sealed class NotificationBusTests
{
    [Fact]
    public void PublishContinuesAfterHandlerException()
    {
        // Arrange
        var bus = new NotificationBus(NullLogger<NotificationBus>.Instance);
        var received = new List<string>();
        bus.Received += static (_, _) => throw new InvalidOperationException();
        bus.Received += (_, e) => received.Add(e.Message);

        // Act
        bus.Publish("message");

        // Assert
        Assert.Equal("message", Assert.Single(received));
    }
}
