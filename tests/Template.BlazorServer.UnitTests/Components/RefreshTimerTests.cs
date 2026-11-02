namespace Template.BlazorServer.Components;

using System.Threading.Channels;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

using Template.BlazorServer.Host.Components;

public sealed class RefreshTimerTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task RequestRefreshesOnNextTick()
    {
        // Arrange
        var time = new FakeTimeProvider();
        var refreshed = new TaskCompletionSource();
        using var timer = new RefreshTimer(NullLogger.Instance, time, TimeSpan.FromMinutes(1), () =>
        {
            refreshed.TrySetResult();
            return Task.CompletedTask;
        }, CancellationToken.None);

        // Act
        timer.Request();
        time.Advance(TimeSpan.FromSeconds(1));

        // Assert
        await refreshed.Task.WaitAsync(Timeout, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RefreshContinuesAfterException()
    {
        // Arrange
        var time = new FakeTimeProvider();
        var calls = Channel.CreateUnbounded<int>();
        var count = 0;
        using var timer = new RefreshTimer(NullLogger.Instance, time, TimeSpan.FromMinutes(1), () =>
        {
            count++;
            calls.Writer.TryWrite(count);
            return count == 1 ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask;
        }, CancellationToken.None);

        // Act
        timer.Request();
        time.Advance(TimeSpan.FromSeconds(1));
        var first = await calls.Reader.ReadAsync(TestContext.Current.CancellationToken).AsTask().WaitAsync(Timeout, TestContext.Current.CancellationToken);
        timer.Request();
        time.Advance(TimeSpan.FromSeconds(1));
        var second = await calls.Reader.ReadAsync(TestContext.Current.CancellationToken).AsTask().WaitAsync(Timeout, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }
}
