namespace Template.BlazorServer.Workers;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

using Template.BlazorServer.Host.Application.Status;
using Template.BlazorServer.Host.Workers;

public sealed class ServerStatusWorkerTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task WorkerAddsSampleWhileWatched()
    {
        // Arrange
        var time = new FakeTimeProvider();
        var monitor = new ServerStatusMonitor(NullLogger<ServerStatusMonitor>.Instance);
        using var worker = new ServerStatusWorker(NullLogger<ServerStatusWorker>.Instance, time, new ServerStatusWorkerOption { Enable = true, IntervalSeconds = 2 }, monitor);
        var updated = new TaskCompletionSource();
        monitor.Updated += (_, _) => updated.TrySetResult();

        // Act
        await worker.StartAsync(TestContext.Current.CancellationToken);
        await AdvanceUntilAsync(time, updated.Task);
        await worker.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(monitor.Samples);
        Assert.InRange(monitor.Current!.CpuPercent, 0, 100);
        Assert.True(worker.ExecuteTask!.IsCompleted);
    }

    [Fact]
    public async Task DisabledWorkerStopsImmediately()
    {
        // Arrange
        var monitor = new ServerStatusMonitor(NullLogger<ServerStatusMonitor>.Instance);
        using var worker = new ServerStatusWorker(NullLogger<ServerStatusWorker>.Instance, new FakeTimeProvider(), new ServerStatusWorkerOption { Enable = false }, monitor);

        // Act
        await worker.StartAsync(TestContext.Current.CancellationToken);
        await worker.ExecuteTask!.WaitAsync(Timeout, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(worker.ExecuteTask.IsCompletedSuccessfully);
    }

    private static async Task AdvanceUntilAsync(FakeTimeProvider time, Task task)
    {
        var limit = DateTime.UtcNow + Timeout;
        while (!task.IsCompleted && (DateTime.UtcNow < limit))
        {
            time.Advance(TimeSpan.FromSeconds(2));
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }

        await task.WaitAsync(TestContext.Current.CancellationToken);
    }
}
