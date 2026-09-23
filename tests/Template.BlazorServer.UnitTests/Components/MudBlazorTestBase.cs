namespace Template.BlazorServer.Components;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using MudBlazor.Services;

using Template.BlazorServer.Host.Application.Context;
using Template.BlazorServer.Services;

public abstract class MudBlazorTestBase : BunitContext
{
    protected MudBlazorTestBase()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        Services.AddSingleton(TimeProvider.System);
        Services.AddSingleton<ApplicationServiceContextProvider>();
        Services.AddSingleton<ServiceContextProvider>(static p => p.GetRequiredService<ApplicationServiceContextProvider>());
        Services.AddScoped<BlazorServiceScope>();
    }
}
