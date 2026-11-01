namespace Template.BlazorServer.Host.Components.Pages.Ignite;

using IgniteUI.Blazor.Controls;

using MudBlazor;

public sealed partial class IgniteIcons
{
    private static readonly (string Name, string Path)[] SvgIcons =
    [
        ("delete", Icons.Material.Filled.Delete),
        ("favorite", Icons.Material.Filled.Favorite),
        ("folder", Icons.Material.Filled.Folder),
        ("home", Icons.Material.Filled.Home),
        ("info", Icons.Material.Filled.Info),
        ("mail", Icons.Material.Filled.Mail),
        ("menu", Icons.Material.Filled.Menu),
        ("person", Icons.Material.Filled.Person),
        ("phone", Icons.Material.Filled.Phone),
        ("search", Icons.Material.Filled.Search),
        ("settings", Icons.Material.Filled.Settings),
        ("star", Icons.Material.Filled.Star)
    ];

#pragma warning disable CA2213
    private IgbIcon icon = default!;
#pragma warning restore CA2213

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await icon.EnsureReady();
            foreach (var (name, path) in SvgIcons)
            {
                await icon.RegisterIconFromTextAsync(name, $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\">{path}</svg>", "default");
            }
        }
    }
}
