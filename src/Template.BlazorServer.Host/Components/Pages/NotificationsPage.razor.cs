namespace Template.BlazorServer.Host.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using MudBlazor;

using Template.BlazorServer.Host.Application.Context;
using Template.BlazorServer.Host.Components.Dialogs;

public sealed partial class NotificationsPage
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);

#pragma warning disable CA2213
    private MudDataGrid<NotificationEntity> grid = default!;
#pragma warning restore CA2213

    private RefreshTimer refreshTimer = default!;

    private string userName = string.Empty;

    private bool unreadOnly;

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
    public required AccountService AccountService { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }

    private bool HasUser => !String.IsNullOrEmpty(userName);

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
        InvokeAsync(() => HasUser ? grid.ReloadServerData() : Task.CompletedTask);

    private Task OnUnreadOnlyChanged(bool value)
    {
        unreadOnly = value;
        return grid.ReloadServerData();
    }

    //--------------------------------------------------------------------------------
    // Grid
    //--------------------------------------------------------------------------------

    private async Task<GridData<NotificationEntity>> LoadServerData(GridState<NotificationEntity> state, CancellationToken cancellationToken)
    {
        var result = await NotificationService.QueryPageAsync(userName, unreadOnly, state.Page, state.PageSize, cancellationToken);
        return new GridData<NotificationEntity>
        {
            TotalItems = result.Total,
            Items = result.Items
        };
    }

    private static string FormatCreatedAt(NotificationEntity notification) => $"{notification.CreatedAt.ToLocalTime():yyyy/MM/dd HH:mm}";

    private static string RowClass(NotificationEntity notification, int index) =>
        notification.ReadAt is null ? "notification-grid-row unread" : "notification-grid-row";

    //--------------------------------------------------------------------------------
    // Action
    //--------------------------------------------------------------------------------

    private async Task OnRowClickAsync(DataGridRowClickEventArgs<NotificationEntity> args)
    {
        var notification = args.Item;
        if (notification.ReadAt is null)
        {
            await NotificationService.MarkReadAsync(notification.Id);
        }

        if (NotificationLink.CanOpen(notification.Link))
        {
            Navigation.NavigateTo(notification.Link);
        }
        else
        {
            await grid.ReloadServerData();
        }
    }

    private async Task MarkAllReadAsync()
    {
        var count = await NotificationService.MarkAllReadAsync();
        if (count > 0)
        {
            Snackbar.AddSuccess($"{count} 件を既読にしました。");
        }
        else
        {
            Snackbar.AddInfo("未読の通知はありません。");
        }

        await grid.ReloadServerData();
    }

    private async Task SendAsync()
    {
        var names = await AccountService.QueryNamesAsync();
        var request = await DialogService.ShowNotificationSendDialog(names);
        if (request is null)
        {
            return;
        }

        var count = await NotificationService.SendAsync(request.UserName, request.Title, request.Body, request.Link);
        if (count > 0)
        {
            Snackbar.AddSuccess($"{count} 人に送りました。");
        }
        else
        {
            Snackbar.AddError("宛先がありません。");
        }
    }
}
