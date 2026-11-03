namespace Template.BlazorServer.Host.Components.Pages.Components;

using BlazorComponents;

using Microsoft.AspNetCore.Components;

public sealed partial class DevicePage
{
    private static readonly TimeSpan LocationTimeout = TimeSpan.FromSeconds(5);

    private bool fullscreen;

    private bool highAccuracy;

    private bool locating;

    private string locationResult = "-";

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required IScreen Screen { get; set; }

    [Inject]
    public required IGeolocation Geolocation { get; set; }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized() =>
        Screen.ScreenKeptOnChanged += OnScreenKeptOnChanged;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Screen.ScreenKeptOnChanged -= OnScreenKeptOnChanged;
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnScreenKeptOnChanged(object? sender, EventArgs e) =>
        _ = InvokeAsync(StateHasChanged);

    private Task KeepScreenOnAsync() => Screen.KeepScreenOnAsync(true).AsTask();

    private Task ReleaseScreenAsync() => Screen.KeepScreenOnAsync(false).AsTask();

    private async Task KeepFullscreenAsync()
    {
        await Screen.KeepFullscreenAsync(true);
        fullscreen = true;
    }

    private async Task ExitFullscreenAsync()
    {
        await Screen.KeepFullscreenAsync(false);
        fullscreen = false;
    }

    private async Task GetLocationAsync()
    {
        locating = true;
        try
        {
            var result = await Geolocation.GetLocationAsync(highAccuracy, LocationTimeout);
            locationResult = result.Position is { } position
                ? $"{result.Status}: {position.Latitude:F5}, {position.Longitude:F5} (±{position.Accuracy:F0} m)"
                : $"{result.Status}";
        }
        finally
        {
            locating = false;
        }
    }
}
