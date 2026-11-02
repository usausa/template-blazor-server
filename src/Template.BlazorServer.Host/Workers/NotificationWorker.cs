namespace Template.BlazorServer.Host.Workers;

using Template.BlazorServer.Host.Infrastructure.Notifications;

public sealed class NotificationWorker : BackgroundService
{
    private readonly ILogger<NotificationWorker> log;

    private readonly TimeProvider timeProvider;

    private readonly NotificationWorkerOption options;

    private readonly NotificationBus bus;

    public NotificationWorker(
        ILogger<NotificationWorker> log,
        TimeProvider timeProvider,
        NotificationWorkerOption options,
        NotificationBus bus)
    {
        this.log = log;
        this.timeProvider = timeProvider;
        this.options = options;
        this.bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enable)
        {
            log.WarnWorkerDisabled(nameof(NotificationWorker));
            return;
        }

        log.InfoWorkerStart(nameof(NotificationWorker));
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.IntervalSeconds), timeProvider);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    bus.Publish($"Server time: {timeProvider.GetLocalNow():HH:mm:ss}");
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    log.ErrorWorkerException(nameof(NotificationWorker), ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown
        }
        finally
        {
            log.InfoWorkerStop(nameof(NotificationWorker));
        }
    }
}
