namespace Template.BlazorServer.Host.Application.Status;

using MusselChart;

public sealed class ServerStatusMonitor
{
    private const int Capacity = 60;

    private readonly ILogger<ServerStatusMonitor> log;

    private volatile ServerStatusSample? current;

    public event EventHandler? Updated;

    public bool IsWatched => Updated is not null;

    public ServerStatusSample? Current => current;

    public ChartRingBuffer<ServerStatusSample> Samples { get; } = new(Capacity);

    public ServerStatusMonitor(ILogger<ServerStatusMonitor> log)
    {
        this.log = log;
    }

    public void Add(ServerStatusSample sample)
    {
        Samples.Push(sample);
        current = sample;

        var handlers = Updated;
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<EventHandler>())
        {
            try
            {
                handler(this, EventArgs.Empty);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.ErrorServerStatusHandlerFailed(ex);
            }
        }
    }

    public void Clear()
    {
        Samples.Clear();
        current = null;
    }
}
