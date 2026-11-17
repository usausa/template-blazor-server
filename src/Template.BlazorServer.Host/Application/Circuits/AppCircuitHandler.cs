namespace Template.BlazorServer.Host.Application.Circuits;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

public sealed class AppCircuitHandler : CircuitHandler, IDisposable
{
    private readonly ILogger<AppCircuitHandler> log;

    private readonly TimeProvider timeProvider;

    private readonly AuthenticationStateProvider authenticationStateProvider;

    private readonly CircuitTracker tracker;

    private string circuitId = string.Empty;

    public AppCircuitHandler(
        ILogger<AppCircuitHandler> log,
        TimeProvider timeProvider,
        AuthenticationStateProvider authenticationStateProvider,
        CircuitTracker tracker)
    {
        this.log = log;
        this.timeProvider = timeProvider;
        this.authenticationStateProvider = authenticationStateProvider;
        this.tracker = tracker;
    }

    public void Dispose()
    {
        authenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        circuitId = circuit.Id;
        authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = state.User.Identity?.Name;
        tracker.Add(new CircuitInfo(circuit.Id, user, timeProvider.GetLocalNow(), true));
        log.InfoCircuitOpened(circuit.Id, user, tracker.Count);
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        tracker.SetConnected(circuit.Id, true);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        tracker.SetConnected(circuit.Id, false);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        tracker.Remove(circuit.Id);
        log.InfoCircuitClosed(circuit.Id, tracker.Count);
        return Task.CompletedTask;
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task) => _ = UpdateUserAsync(task);

    private async Task UpdateUserAsync(Task<AuthenticationState> task)
    {
        try
        {
            var state = await task;
            tracker.SetUser(circuitId, state.User.Identity?.Name);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.ErrorCircuitUserUpdateFailed(circuitId, ex);
        }
    }
}
