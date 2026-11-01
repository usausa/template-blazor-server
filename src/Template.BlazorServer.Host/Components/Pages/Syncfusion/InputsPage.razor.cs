namespace Template.BlazorServer.Host.Components.Pages.Syncfusion;

using global::Syncfusion.Blazor.Toolkit;

public sealed partial class InputsPage
{
    private string? name;

    private string? notes;

    private int quantity = 1;

    private decimal price = 9.5m;

    private string[] files = [];

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnFileValueChange(UploadChangeEventArgs args) =>
        files = [.. (args.Files ?? []).Select(static x => $"{x.FileInfo.Name} ({x.FileInfo.Size:N0} bytes)")];

    private void OnFileClear(ClearingEventArgs args) => files = [];
}
