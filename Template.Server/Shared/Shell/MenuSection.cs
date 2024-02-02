namespace Template.Server.Shared.Shell;

using Microsoft.AspNetCore.Components;

public sealed class MenuSection : ComponentBase, IDisposable
{
    [CascadingParameter]
    public required IMenuSectionCallback Callback { get; set; }

    [Parameter]
    public required RenderFragment ChildContent { get; set; }

    protected override void OnInitialized()
    {
        Callback.SetMenu(ChildContent);
    }

    public void Dispose()
    {
        Callback.SetMenu(null);
    }
}
