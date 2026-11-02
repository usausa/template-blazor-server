namespace Template.BlazorServer.Host.Components.Pages;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Template.BlazorServer.Host.Application.Circuits;
using Template.BlazorServer.Host.Application.Status;
using Template.BlazorServer.Infrastructure.Storage;

public sealed partial class ServerPage
{
    private const int LabelStep = 10;

    private const double Megabyte = 1024d * 1024d;

    private const double Gigabyte = 1024d * 1024d * 1024d;

    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(10);

    private readonly LineChartOptions cpuOptions = new()
    {
        ChartPalette = ["#5794F2"],
        ShowLegend = false,
        YAxisRequireZeroPoint = true,
        YAxisSuggestedMax = 100
    };

    private readonly LineChartOptions memoryOptions = new()
    {
        ChartPalette = ["#5794F2", "#3274D9"],
        YAxisRequireZeroPoint = true
    };

    private RefreshTimer refreshTimer = default!;

    private ServerStatusSample? current;

    private string[] chartLabels = [];

    private List<ChartSeries<double>> cpuSeries = [];

    private List<ChartSeries<double>> memorySeries = [];

    private IReadOnlyList<CircuitInfo> circuits = [];

    private string diskUsage = string.Empty;

    private int dataCount;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required RefreshTimerFactory RefreshTimerFactory { get; set; }

    [Inject]
    public required ServerStatusMonitor Monitor { get; set; }

    [Inject]
    public required CircuitTracker CircuitTracker { get; set; }

    [Inject]
    public required DataService DataService { get; set; }

    [Inject]
    public required DataChangeNotifier DataChangeNotifier { get; set; }

    [Inject]
    public required FileStorageOption StorageOptions { get; set; }

    private bool HasSamples => chartLabels.Length > 0;

    private string CpuText => current is null ? "-" : $"{current.CpuPercent:0.0} %";

    private string MemoryText => current is null ? "-" : $"{current.WorkingSet / Megabyte:#,0} MB";

    private string GcText => current is null ? "-" : $"{current.Gen0Collections} / {current.Gen1Collections} / {current.Gen2Collections}";

    private string ThreadText => current is null ? "-" : $"{current.ThreadCount}";

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        refreshTimer = RefreshTimerFactory.Create(RefreshInterval, RefreshAsync);
        Monitor.Updated += OnStatusUpdated;
        CircuitTracker.Changed += OnCircuitChanged;
        DataChangeNotifier.Changed += OnDataChanged;
    }

    protected override Task OnInitializedAsync() => LoadAsync();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Monitor.Updated -= OnStatusUpdated;
            CircuitTracker.Changed -= OnCircuitChanged;
            DataChangeNotifier.Changed -= OnDataChanged;
            refreshTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnStatusUpdated(object? sender, EventArgs e) => refreshTimer.Request();

    private void OnCircuitChanged(object? sender, EventArgs e) => refreshTimer.Request();

    private void OnDataChanged(object? sender, EventArgs e) => refreshTimer.Request();

    private Task RefreshAsync() =>
        InvokeAsync(async () =>
        {
            await LoadAsync();
            StateHasChanged();
        });

    //--------------------------------------------------------------------------------
    // Load
    //--------------------------------------------------------------------------------

    private async Task LoadAsync()
    {
        var history = Monitor.History();
        current = Monitor.Current;
        chartLabels = history.Select((x, i) => (history.Count - 1 - i) % LabelStep == 0 ? $"{x.Time:HH:mm:ss}" : string.Empty).ToArray();
        cpuSeries = [new ChartSeries<double> { Name = "CPU", Data = history.Select(static x => x.CpuPercent).ToArray() }];
        memorySeries =
        [
            new ChartSeries<double> { Name = "ワーキングセット", Data = history.Select(static x => Math.Round(x.WorkingSet / Megabyte, 1)).ToArray() },
            new ChartSeries<double> { Name = "GC ヒープ", Data = history.Select(static x => Math.Round(x.HeapSize / Megabyte, 1)).ToArray() }
        ];
        circuits = CircuitTracker.List();
        diskUsage = MakeDiskUsage();
        dataCount = await DataService.CountAsync(null);
    }

    private string MakeDiskUsage()
    {
        var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(StorageOptions.Root))!);
        var used = drive.TotalSize - drive.AvailableFreeSpace;
        return $"{used / Gigabyte:#,0} / {drive.TotalSize / Gigabyte:#,0} GB";
    }

    //--------------------------------------------------------------------------------
    // Format
    //--------------------------------------------------------------------------------

    private static string FormatUser(CircuitInfo circuit) => circuit.User ?? "-";

    private static string FormatOpenedAt(CircuitInfo circuit) => $"{circuit.OpenedAt:yyyy/MM/dd HH:mm:ss}";

    private static string ConnectedText(CircuitInfo circuit) => circuit.Connected ? "接続中" : "切断中";

    private static Color ConnectedColor(CircuitInfo circuit) => circuit.Connected ? Color.Primary : Color.Default;
}
