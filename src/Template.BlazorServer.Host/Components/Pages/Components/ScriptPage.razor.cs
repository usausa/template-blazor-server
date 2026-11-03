namespace Template.BlazorServer.Host.Components.Pages.Components;

using BlazorComponents;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Template.BlazorServer.Host.Reports;

public sealed partial class ScriptPage : IAsyncDisposable
{
    private const string FocusTargetId = "sample-focus-target";

    private const string ModulePath = "./js/sample-module.js";

    private const int PdfRows = 10;

    private ElementReference box;

    private ScriptModule? module;

    private string measureResult = "-";

    private string fileResult = "-";

    private string copyText = "Copied from Components.Blazor";

    private string copyResult = "-";

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IClipboard Clipboard { get; set; }

    [Inject]
    public required ProductListReportBuilder ProductListReportBuilder { get; set; }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            await module.DisposeAsync();
        }

        Dispose();
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private async Task MeasureAsync()
    {
        module ??= new ScriptModule(JSRuntime, ModulePath);
        var size = await module.InvokeAsync<MeasureResult>("measure", box);
        measureResult = $"{size.Width} x {size.Height} px, {size.Language}";
    }

    private Task FocusAsync() => JSRuntime.SetFocus(FocusTargetId).AsTask();

    private Task BackAsync() => JSRuntime.HistoryBack().AsTask();

    private async Task SaveCsvAsync()
    {
        var csv = String.Join("\r\n", ProductSample.LoadItems(3).Select(static x => $"{x.Code},{x.Name},{x.Quantity}").Prepend("Code,Name,Quantity"));
        await JSRuntime.SaveAsFile("sample.csv", "text/csv", [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(csv)]);
        fileResult = "Saved: sample.csv";
    }

    private async Task OpenPdfAsync()
    {
        var bytes = ProductListReportBuilder.Build(ProductSample.LoadItems(PdfRows));
        await JSRuntime.OpenNewWindow("application/pdf", bytes);
        fileResult = $"Opened: {bytes.Length:N0} bytes";
    }

    private async Task CopyAsync()
    {
        var copied = await Clipboard.SetTextAsync(copyText);
        copyResult = copied ? "Copied" : "Not copied";
    }

    //--------------------------------------------------------------------------------
    // View
    //--------------------------------------------------------------------------------

    private sealed record MeasureResult(int Width, int Height, string Language);
}
