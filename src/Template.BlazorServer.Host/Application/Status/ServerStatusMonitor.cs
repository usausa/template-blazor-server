namespace Template.BlazorServer.Host.Application.Status;

public sealed class ServerStatusMonitor
{
    private const int Capacity = 60;

    private readonly Lock sync = new();

    private readonly Queue<ServerStatusSample> samples = new();

    private readonly ILogger<ServerStatusMonitor> log;

    private volatile ServerStatusSample? current;

    public event EventHandler? Updated;

    public bool IsWatched => Updated is not null;

    public ServerStatusSample? Current => current;

    public ServerStatusMonitor(ILogger<ServerStatusMonitor> log)
    {
        this.log = log;
    }

    public IReadOnlyList<ServerStatusSample> History()
    {
        lock (sync)
        {
            return samples.ToArray();
        }
    }

    public void Add(ServerStatusSample sample)
    {
        lock (sync)
        {
            samples.Enqueue(sample);
            while (samples.Count > Capacity)
            {
                samples.Dequeue();
            }

            current = sample;
        }

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
        lock (sync)
        {
            samples.Clear();
            current = null;
        }
    }
}
