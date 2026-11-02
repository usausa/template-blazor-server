namespace Template.BlazorServer.Host.Workers;

public sealed class ServerStatusWorkerOption
{
    public bool Enable { get; set; }

    [Range(1, 3600)]
    public int IntervalSeconds { get; set; } = 2;
}
