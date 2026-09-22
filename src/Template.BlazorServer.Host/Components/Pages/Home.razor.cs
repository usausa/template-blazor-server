namespace Template.BlazorServer.Host.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.FeatureManagement;

using MudBlazor;

using Template.BlazorServer.Host.Application;
using Template.BlazorServer.Host.Infrastructure.Circuits;
using Template.BlazorServer.Host.Infrastructure.Components;
using Template.BlazorServer.Host.Infrastructure.Notifications;

public sealed partial class Home
{
    private int circuitCount;

    private string? lastNotification;

    private bool featureEnabled;

    [Inject]
    public required NotificationBus NotificationBus { get; set; }

    [Inject]
    public required CircuitTracker CircuitTracker { get; set; }

    [Inject]
    public required IFeatureManager FeatureManager { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Subscribe server notification (unsubscribed on dispose)
        NotificationBus.Received += OnNotificationReceived;
        CircuitTracker.Changed += OnCircuitChanged;
        circuitCount = CircuitTracker.Count;

        // Feature flag example
        featureEnabled = await FeatureManager.IsEnabledAsync(FeatureFlags.CustomOption);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            NotificationBus.Received -= OnNotificationReceived;
            CircuitTracker.Changed -= OnCircuitChanged;
        }

        base.Dispose(disposing);
    }

    private void OnCircuitChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(() =>
        {
            circuitCount = CircuitTracker.Count;
            StateHasChanged();
        });
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
