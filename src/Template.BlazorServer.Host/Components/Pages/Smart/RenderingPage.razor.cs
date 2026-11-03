namespace Template.BlazorServer.Host.Components.Pages.Smart;

using global::Smart.AspNetCore.Components;

public sealed partial class RenderingPage
{
    private static readonly TimeSpan LoadDelay = TimeSpan.FromSeconds(1);

    private static readonly string[] Cities = ["Tokyo", "Osaka", "Nagoya", "Fukuoka", "Sapporo"];

    private LoadingStatus status = LoadingStatus.NotLoaded;

    private string[] cities = [];

    private string? selected = Cities[0];

    private int count = 5;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    private bool HasSelected => selected is not null;

    private string SelectedText => selected ?? "null";

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private Task LoadAsync() => LoadAsync(false);

    private Task LoadFailedAsync() => LoadAsync(true);

    private void Reset()
    {
        status = LoadingStatus.NotLoaded;
        cities = [];
    }

    private void OnHasSelectedChanged(bool value) => selected = value ? Cities[0] : null;

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private async Task LoadAsync(bool fail)
    {
        status = LoadingStatus.Loading;
        cities = [];
        await Task.Delay(LoadDelay);

        if (fail)
        {
            status = LoadingStatus.Failed;
            return;
        }

        cities = Cities;
        status = LoadingStatus.Loaded;
    }
}
