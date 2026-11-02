namespace Template.BlazorServer.Host.Components.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using MudBlazor;

using Template.BlazorServer.Host.Application.Context;

public sealed partial class NotificationBell
{
    private const int LatestCount = 5;

    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

    private RefreshTimer refreshTimer = default!;

    private string userName = string.Empty;

    private int unreadCount;

    private List<NotificationEntity> latest = [];

    private long lastId;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required RefreshTimerFactory RefreshTimerFactory { get; set; }

    [Inject]
    public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    [Inject]
    public required NotificationService NotificationService { get; set; }

    [Inject]
    public required NotificationChangeNotifier NotificationChangeNotifier { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    private bool HasUnread => unreadCount > 0;

    private bool HasLatest => latest.Count > 0;

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        refreshTimer = RefreshTimerFactory.Create(RefreshInterval, RefreshAsync);
        NotificationChangeNotifier.Changed += OnNotificationChanged;
    }

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        userName = HttpServiceContext.GetUserId(state.User);
        await LoadAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            NotificationChangeNotifier.Changed -= OnNotificationChanged;
            refreshTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnNotificationChanged(object? sender, NotificationChangedEventArgs e)
    {
        if ((e.UserName is null) || (e.UserName == userName))
        {
            refreshTimer.Request();
        }
    }

    private Task RefreshAsync() =>
        InvokeAsync(async () =>
        {
            var previousId = lastId;
            await LoadAsync();
            ShowReceived(previousId);
            StateHasChanged();
        });

    //--------------------------------------------------------------------------------
    // Action
    //--------------------------------------------------------------------------------

    private async Task OpenAsync(NotificationEntity notification)
    {
        if (notification.ReadAt is null)
        {
            await NotificationService.MarkReadAsync(notification.Id);
        }

        Navigation.NavigateTo(NotificationLink.CanOpen(notification.Link) ? notification.Link : "notifications");
    }

    //--------------------------------------------------------------------------------
    // Load
    //--------------------------------------------------------------------------------

    private async Task LoadAsync()
    {
        unreadCount = await NotificationService.CountUnreadAsync(userName);
        latest = await NotificationService.QueryLatestAsync(userName, LatestCount);
        if (latest.Count > 0)
        {
            lastId = Math.Max(lastId, latest[0].Id);
        }
    }

    private void ShowReceived(long previousId)
    {
        var received = latest.Where(x => (x.Id > previousId) && (x.ReadAt is null)).ToList();
        if (received.Count == 1)
        {
            Snackbar.AddInfo($"新しい通知: {received[0].Title}");
        }
        else if (received.Count > 1)
        {
            Snackbar.AddInfo($"新しい通知が {received.Count} 件あります。");
        }
    }

    //--------------------------------------------------------------------------------
    // Format
    //--------------------------------------------------------------------------------

    private static string RowClass(NotificationEntity notification) =>
        notification.ReadAt is null ? "notification-row unread" : "notification-row";

    private static string FormatTime(NotificationEntity notification) => $"{notification.CreatedAt:MM/dd HH:mm}";
}
