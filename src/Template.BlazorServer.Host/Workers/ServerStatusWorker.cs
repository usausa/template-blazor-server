namespace Template.BlazorServer.Host.Workers;

using Template.BlazorServer.Host.Application.Status;

public sealed class ServerStatusWorker : BackgroundService
{
    private readonly ILogger<ServerStatusWorker> log;

    private readonly TimeProvider timeProvider;

    private readonly ServerStatusWorkerOption options;

    private readonly ServerStatusMonitor monitor;

    public ServerStatusWorker(
        ILogger<ServerStatusWorker> log,
        TimeProvider timeProvider,
        ServerStatusWorkerOption options,
        ServerStatusMonitor monitor)
    {
        this.log = log;
        this.timeProvider = timeProvider;
        this.options = options;
        this.monitor = monitor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enable)
        {
            log.WarnWorkerDisabled(nameof(ServerStatusWorker));
            return;
        }

        log.InfoWorkerStart(nameof(ServerStatusWorker));
        try
        {
            var lastCpuTime = Environment.CpuUsage.TotalTime;
            var lastTimestamp = timeProvider.GetTimestamp();

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.IntervalSeconds), timeProvider);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var cpuTime = Environment.CpuUsage.TotalTime;
                var timestamp = timeProvider.GetTimestamp();
                var elapsed = timeProvider.GetElapsedTime(lastTimestamp, timestamp);
                var cpu = elapsed > TimeSpan.Zero
                    ? Math.Clamp((cpuTime - lastCpuTime) / elapsed / Environment.ProcessorCount * 100, 0, 100)
                    : 0;
                lastCpuTime = cpuTime;
                lastTimestamp = timestamp;

                if (!monitor.IsWatched)
                {
                    monitor.Clear();
                    continue;
                }

                try
                {
                    monitor.Add(new ServerStatusSample(
                        timeProvider.GetLocalNow(),
                        Math.Round(cpu, 1),
                        Environment.WorkingSet,
                        GC.GetTotalMemory(false),
                        GC.CollectionCount(0),
                        GC.CollectionCount(1),
                        GC.CollectionCount(2),
                        ThreadPool.ThreadCount));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    log.ErrorWorkerException(nameof(ServerStatusWorker), ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown
        }
        finally
        {
            log.InfoWorkerStop(nameof(ServerStatusWorker));
        }
    }
}
