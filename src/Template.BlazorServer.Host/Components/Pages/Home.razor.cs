namespace Template.BlazorServer.Host.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.FeatureManagement;

using MudBlazor;

using Template.BlazorServer.Host.Application;
using Template.BlazorServer.Host.Infrastructure.Components;
using Template.BlazorServer.Host.Infrastructure.Notifications;

public sealed partial class Home
{
    private string? lastNotification;

    private bool featureEnabled;

    [Inject]
    public required NotificationBus NotificationBus { get; set; }

    [Inject]
    public required IFeatureManager FeatureManager { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Subscribe server notification (unsubscribed on dispose)
        NotificationBus.Received += OnNotificationReceived;

        // Feature flag example
        featureEnabled = await FeatureManager.IsEnabledAsync(FeatureFlags.CustomOption);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            NotificationBus.Received -= OnNotificationReceived;
        }

        base.Dispose(disposing);
    }

    private void OnNotificationReceived(object? sender, NotificationEventArgs e)
    {
        _ = InvokeAsync(() =>
        {
            lastNotification = e.Message;
            Snackbar.AddInfo(e.Message);
            StateHasChanged();
        });
    }
}
