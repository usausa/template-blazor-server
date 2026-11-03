namespace Template.BlazorServer.Host.Components.Pages.Components;

using BlazorComponents;

using Microsoft.AspNetCore.Components;

public sealed partial class TabsPage : IAsyncDisposable
{
    private const string ChannelName = "sample-channel";

    private const string LockName = "sample-lock";

    private const int LogSize = 10;

    private static readonly TimeSpan WorkTime = TimeSpan.FromSeconds(3);

    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(1);

    private readonly List<string> lockLog = [];

    private readonly List<string> received = [];

    private IAsyncDisposable? listening;

    private string message = "Hello";

    private int runCount;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required IBrowserTabs Tabs { get; set; }

    [Inject]
    public required TimeProvider TimeProvider { get; set; }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            listening = await Tabs.ListenAsync(ChannelName, OnReceivedAsync);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (listening is not null)
        {
            await listening.DisposeAsync();
        }

        Dispose();
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private Task OnReceivedAsync(string value) =>
        InvokeAsync(() =>
        {
            Add(received, $"{Now()} {value}");
            StateHasChanged();
        });

    private Task RunLockedAsync() => RunAsync(null);

    private Task RunLockedWithTimeoutAsync() => RunAsync(LockTimeout);

    private Task TellAsync() => Tabs.TellAsync(ChannelName, message).AsTask();

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private async Task RunAsync(TimeSpan? timeout)
    {
        Add(lockLog, $"{Now()} requested");
        await Tabs.RunLockedAsync(LockName, async () =>
        {
            Add(lockLog, $"{Now()} running");
            await InvokeAsync(StateHasChanged);
            await Task.Delay(WorkTime);
        }, timeout);
        runCount++;
        Add(lockLog, $"{Now()} done");
    }

    private string Now() => $"{TimeProvider.GetLocalNow():HH:mm:ss.f}";

    private static void Add(List<string> list, string entry)
    {
        list.Insert(0, entry);
        if (list.Count > LogSize)
        {
            list.RemoveAt(list.Count - 1);
        }
    }
}
