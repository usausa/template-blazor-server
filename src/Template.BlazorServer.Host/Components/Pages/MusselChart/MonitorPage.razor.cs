namespace Template.BlazorServer.Host.Components.Pages.MusselChart;

using global::MusselChart;

using Microsoft.AspNetCore.Components;

public sealed partial class MonitorPage
{
    private static readonly TimeSpan WideWindow = TimeSpan.FromMinutes(3);

    private static readonly TimeSpan NarrowWindow = TimeSpan.FromSeconds(45);

    private static readonly ChartThreshold[] DiskThresholds =
    [
        new() { Level = ChartThresholdLevel.Good },
        new() { Value = 70, Level = ChartThresholdLevel.Warning },
        new() { Value = 85, Level = ChartThresholdLevel.Critical }
    ];

    private bool wide = true;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required MonitorFeed Feed { get; set; }

    private TimeSpan Window => wide ? WideWindow : NarrowWindow;

    private string WindowText => wide ? "3 min" : "45 s";

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        Feed.Updated += OnUpdated;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Feed.Updated -= OnUpdated;
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnUpdated(object? sender, EventArgs e) => _ = InvokeAsync(StateHasChanged);

    private void ToggleWindow() => wide = !wide;
}
