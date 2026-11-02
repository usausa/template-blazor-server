namespace Template.BlazorServer.Services;

using Microsoft.Extensions.Logging.Abstractions;

public sealed class NotificationChangeNotifierTests
{
    [Fact]
    public void NotifyContinuesAfterHandlerException()
    {
        // Arrange
        var notifier = new NotificationChangeNotifier(NullLogger<NotificationChangeNotifier>.Instance);
        var received = new List<string?>();
        notifier.Changed += static (_, _) => throw new InvalidOperationException();
        notifier.Changed += (_, e) => received.Add(e.UserName);

        // Act
        notifier.Notify("admin");

        // Assert
        Assert.Equal("admin", Assert.Single(received));
    }
}
