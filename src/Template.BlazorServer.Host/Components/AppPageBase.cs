namespace Template.BlazorServer.Host.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using Template.BlazorServer.Host.Application.Context;

public abstract class AppPageBase : AppComponentBase, IHandleEvent
{
    [Inject]
    private BlazorServiceScope ServiceScope { get; set; } = default!;

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationState { get; set; } = default!;

    Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem item, object? arg) => HandleEventAsync(item, arg);

    protected async Task HandleEventAsync(EventCallbackWorkItem callback, object? arg)
    {
        using var scope = ServiceScope.Begin(await ServiceScope.PrepareAsync(AuthenticationState));

        var task = callback.InvokeAsync(arg);
        var shouldAwait = task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Canceled;
        StateHasChanged();
        if (shouldAwait)
        {
            try
            {
                await task;
            }
            catch
            {
                if (task.IsCanceled)
                {
                    return;
                }

                throw;
            }

            StateHasChanged();
        }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var authenticationState = parameters.GetValueOrDefault(nameof(AuthenticationState), AuthenticationState);
        using var scope = ServiceScope.Begin(await ServiceScope.PrepareAsync(authenticationState));
        await base.SetParametersAsync(parameters);
    }
}
