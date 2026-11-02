namespace Template.BlazorServer.Services;

using Microsoft.Extensions.Logging.Abstractions;

public sealed class DataChangeNotifierTests
{
    [Fact]
    public void NotifyContinuesAfterHandlerException()
    {
        // Arrange
        var notifier = new DataChangeNotifier(NullLogger<DataChangeNotifier>.Instance);
        var count = 0;
        notifier.Changed += static (_, _) => throw new InvalidOperationException();
        notifier.Changed += (_, _) => count++;

        // Act
        notifier.Notify();

        // Assert
        Assert.Equal(1, count);
    }
}
