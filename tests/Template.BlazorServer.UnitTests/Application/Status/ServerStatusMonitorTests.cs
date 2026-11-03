namespace Template.BlazorServer.Application.Status;

using Microsoft.Extensions.Logging.Abstractions;

using Template.BlazorServer.Host.Application.Status;

public sealed class ServerStatusMonitorTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddKeepsLatestSamples()
    {
        // Arrange
        var monitor = new ServerStatusMonitor(NullLogger<ServerStatusMonitor>.Instance);

        // Act
        for (var i = 0; i <= 60; i++)
        {
            monitor.Add(MakeSample(i));
        }

        // Assert
        var history = monitor.Samples.ToArray();
        Assert.Equal(60, history.Length);
        Assert.Equal(Now.AddSeconds(1), history[0].Time);
        Assert.Same(history[^1], monitor.Current);
    }

    [Fact]
    public void IsWatchedFollowsSubscription()
    {
        // Arrange
        var monitor = new ServerStatusMonitor(NullLogger<ServerStatusMonitor>.Instance);

        // Act
        var before = monitor.IsWatched;
        monitor.Updated += OnUpdated;
        var watched = monitor.IsWatched;
        monitor.Updated -= OnUpdated;

        // Assert
        Assert.False(before);
        Assert.True(watched);
        Assert.False(monitor.IsWatched);

        static void OnUpdated(object? sender, EventArgs e)
        {
        }
    }

    [Fact]
    public void AddContinuesAfterHandlerException()
    {
        // Arrange
        var monitor = new ServerStatusMonitor(NullLogger<ServerStatusMonitor>.Instance);
        var count = 0;
        monitor.Updated += static (_, _) => throw new InvalidOperationException();
        monitor.Updated += (_, _) => count++;

        // Act
        monitor.Add(MakeSample(0));

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public void ClearRemovesSamples()
    {
        // Arrange
        var monitor = new ServerStatusMonitor(NullLogger<ServerStatusMonitor>.Instance);
        monitor.Add(MakeSample(0));

        // Act
        monitor.Clear();

        // Assert
        Assert.Empty(monitor.Samples);
        Assert.Null(monitor.Current);
    }

    private static ServerStatusSample MakeSample(int second) =>
        new(Now.AddSeconds(second), 10, 100, 50, 3, 2, 1, 4);
}
