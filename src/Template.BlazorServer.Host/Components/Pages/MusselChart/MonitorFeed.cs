namespace Template.BlazorServer.Host.Components.Pages.MusselChart;

using global::MusselChart;

public sealed class MonitorFeed : IDisposable
{
    private const int HistoryCount = 190;

    private const int TrendCount = 30;

    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    private readonly Lock sync = new();

    private readonly TimeProvider timeProvider;

    private readonly double[] loads = [28, 21, 34, 17];

    private EventHandler? updated;

    private CancellationTokenSource? cts;

    private bool disposed;

    private int step;

    private double receive = 220;

    private double send = 90;

    public event EventHandler? Updated
    {
        add
        {
            lock (sync)
            {
                if ((updated is null) && !disposed)
                {
                    Fill();
                    cts = new CancellationTokenSource();
                    _ = RunAsync(cts.Token);
                }

                updated += value;
            }
        }
        remove
        {
            lock (sync)
            {
                updated -= value;
                if (updated is null)
                {
                    Stop();
                }
            }
        }
    }

    public ChartRingBuffer<MonitorSample> Samples { get; } = new(HistoryCount);

    public ChartRingBuffer<double> CpuTrend { get; } = new(TrendCount);

    public ChartRingBuffer<double> MemoryTrend { get; } = new(TrendCount);

    public ChartRingBuffer<double> TemperatureTrend { get; } = new(TrendCount);

    public ChartRingBuffer<double> NetworkTrend { get; } = new(TrendCount);

    public IReadOnlyList<ChartEntry> Disks { get; } =
    [
        new("C: System", 57, null, 100),
        new("D: Data", 23, null, 100),
        new("E: Work", 67, null, 100),
        new("F: Backup", 82, null, 100)
    ];

    public double Cpu { get; private set; }

    public double Memory { get; private set; } = 53;

    public double Temperature { get; private set; } = 54;

    public double Network { get; private set; }

    public MonitorFeed(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public void Dispose()
    {
        lock (sync)
        {
            disposed = true;
            updated = null;
            Stop();
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timer = new PeriodicTimer(Interval, timeProvider);
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                EventHandler? handlers;
                lock (sync)
                {
                    Add(timeProvider.GetUtcNow().UtcDateTime);
                    handlers = updated;
                }

                handlers?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (OperationCanceledException)
        {
            // Stopped
        }
    }

    private void Stop()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    private void Fill()
    {
        var time = timeProvider.GetUtcNow().UtcDateTime - (Interval * (HistoryCount - 1));
        for (var i = 0; i < HistoryCount; i++)
        {
            Add(time);
            time += Interval;
        }
    }

    private void Add(DateTime time)
    {
        step++;

        for (var i = 0; i < loads.Length; i++)
        {
            var wave = Math.Sin((step * 0.11) + (i * 1.3)) * 14;
            var drift = ((step * (7 + i)) % 13) - 6;
            loads[i] = Math.Clamp(((loads[i] * 2) + 30 + wave + drift) / 3, 2, 99);
        }

        Cpu = loads.Average();
        Memory = Math.Clamp(Memory + ((((step * 5) % 7) - 3) * 0.15), 38, 72);
        Temperature = Math.Clamp(Temperature + ((Cpu - 35) * 0.04) + ((((step * 3) % 5) - 2) * 0.2), 40, 92);
        receive = Math.Clamp((receive * 0.7) + (Math.Abs(Math.Sin(step * 0.17)) * 420) + (((step * 13) % 17) * 3), 5, 1200);
        send = Math.Clamp((send * 0.7) + (Math.Abs(Math.Cos(step * 0.23)) * 160) + (((step * 7) % 11) * 2), 2, 600);
        Network = receive + send;

        var queue = Math.Round(Math.Max(0, (Cpu / 18) + (((step * 5) % 7) - 2)));
        Samples.Push(new MonitorSample(time, loads[0], loads[1], loads[2], loads[3], receive, send, queue));
        CpuTrend.Push(Cpu);
        MemoryTrend.Push(Memory);
        TemperatureTrend.Push(Temperature);
        NetworkTrend.Push(Network);
    }
}
