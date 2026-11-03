namespace Template.BlazorServer.Components.Pages.MusselChart;

using Microsoft.Extensions.Time.Testing;

using Template.BlazorServer.Host.Components.Pages.MusselChart;

public sealed class MonitorFeedTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 0, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public void SubscribeFillsHistory()
    {
        // Arrange
        using var feed = new MonitorFeed(new FakeTimeProvider(Now));

        // Act
        feed.Updated += OnUpdated;

        // Assert
        Assert.Equal(190, feed.Samples.Count);
        Assert.Equal(Now.UtcDateTime, feed.Samples[^1].Time);
        Assert.Equal(30, feed.CpuTrend.Count);
        Assert.InRange(feed.Cpu, 2, 99);
    }

    [Fact]
    public async Task TickAddsSample()
    {
        // Arrange
        var time = new FakeTimeProvider(Now);
        using var feed = new MonitorFeed(time);
        var updated = new TaskCompletionSource();
        feed.Updated += (_, _) => updated.TrySetResult();

        // Act
        time.Advance(TimeSpan.FromSeconds(1));
        await updated.Task.WaitAsync(Timeout, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(191, feed.Samples.Sequence);
        Assert.Equal(Now.UtcDateTime.AddSeconds(1), feed.Samples[^1].Time);
    }

    [Fact]
    public async Task UnsubscribeStopsFeed()
    {
        // Arrange
        var time = new FakeTimeProvider(Now);
        using var feed = new MonitorFeed(time);
        feed.Updated += OnUpdated;

        // Act
        feed.Updated -= OnUpdated;
        time.Advance(TimeSpan.FromSeconds(5));
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(190, feed.Samples.Sequence);
    }

    private static void OnUpdated(object? sender, EventArgs e)
    {
    }
}
