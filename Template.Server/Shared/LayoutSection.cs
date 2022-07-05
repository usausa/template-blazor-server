namespace Template.Server.Shared;

using Microsoft.AspNetCore.Components;

public sealed class LayoutSection : ComponentBase, IDisposable
{
    [CascadingParameter]
    public ISectionCallback Callback { get; set; } = default!;

    [Parameter]
    public RenderFragment Menu { get; set; } = default!;

    protected override void OnInitialized()
    {
        Callback.SetMenu(Menu);
    }

    public void Dispose()
    {
        Callback.SetMenu(null);
    }
}
