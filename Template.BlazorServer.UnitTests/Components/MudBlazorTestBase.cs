namespace Template.BlazorServer.Components;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using MudBlazor.Services;

public abstract class MudBlazorTestBase : BunitContext
{
    protected MudBlazorTestBase()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
