namespace Template.BlazorServer.Host.Workers;

public sealed class NotificationRetentionWorker : BackgroundService
{
    private readonly ILogger<NotificationRetentionWorker> log;

    private readonly TimeProvider timeProvider;

    private readonly NotificationRetentionWorkerOption options;

    private readonly NotificationService notificationService;

    public NotificationRetentionWorker(
        ILogger<NotificationRetentionWorker> log,
        TimeProvider timeProvider,
        NotificationRetentionWorkerOption options,
        NotificationService notificationService)
    {
        this.log = log;
        this.timeProvider = timeProvider;
        this.options = options;
        this.notificationService = notificationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enable)
        {
            log.WarnWorkerDisabled(nameof(NotificationRetentionWorker));
            return;
        }

        log.InfoWorkerStart(nameof(NotificationRetentionWorker));
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(options.IntervalMinutes), timeProvider);
            do
            {
                await DeleteExpiredAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Shutdown
        }
        finally
        {
            log.InfoWorkerStop(nameof(NotificationRetentionWorker));
        }
    }

    private async Task DeleteExpiredAsync(CancellationToken cancellationToken)
    {
        try
        {
            var before = timeProvider.GetLocalNow().AddDays(-options.Days).DateTime;
            var total = 0;
            int rows;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                rows = await notificationService.DeleteBeforeAsync(before, options.BatchSize);
                total += rows;
            }
            while (rows >= options.BatchSize);

            if (total > 0)
            {
                log.InfoNotificationExpired(total);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.ErrorWorkerException(nameof(NotificationRetentionWorker), ex);
        }
    }
}
