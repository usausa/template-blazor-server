namespace Template.BlazorServer.Host.Workers;

public sealed class NotificationRetentionWorkerOption
{
    public bool Enable { get; set; }

    [Range(1, 1440)]
    public int IntervalMinutes { get; set; } = 60;

    [Range(1, 3650)]
    public int Days { get; set; } = 30;

    [Range(1, 100_000)]
    public int BatchSize { get; set; } = 1000;
}
