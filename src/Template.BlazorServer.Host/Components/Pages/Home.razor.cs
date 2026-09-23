namespace Template.BlazorServer.Host.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.FeatureManagement;

using MudBlazor;

using Template.BlazorServer.Host.Application;
using Template.BlazorServer.Host.Application.Circuits;
using Template.BlazorServer.Host.Infrastructure.Notifications;

public sealed partial class Home
{
    private int circuitCount;

    private string? lastNotification;

    private bool HasNotification => !String.IsNullOrEmpty(lastNotification);

    private bool featureEnabled;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required NotificationBus NotificationBus { get; set; }

    [Inject]
    public required CircuitTracker CircuitTracker { get; set; }

    [Inject]
    public required IFeatureManager FeatureManager { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override async Task OnInitializedAsync()
    {
        NotificationBus.Received += OnNotificationReceived;
        CircuitTracker.Changed += OnCircuitChanged;
        circuitCount = CircuitTracker.Count;

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

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

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
