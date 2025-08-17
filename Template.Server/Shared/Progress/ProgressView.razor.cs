namespace Template.Server.Shared.Progress;

public sealed partial class ProgressView : IDisposable
{
    private readonly ProgressState progress = new();

    [Inject]
    public required IJSRuntime Script { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string visibility = "hidden";

    protected override void OnInitialized()
    {
        progress.StateChanged += OnStateChanged;
    }

    public void Dispose()
    {
        progress.StateChanged -= OnStateChanged;
    }

    // ReSharper disable once AsyncVoidEventHandlerMethod
    private async void OnStateChanged(object? sender, EventArgs e)
    {
        StateHasChanged();
        visibility = progress.IsBusy ? "visible" : "hidden";
        await Script.InvokeVoidAsync(progress.IsBusy ? "Progress.showProgress" : "Progress.hideProgress", "progress-modal");
        StateHasChanged();
    }
}
