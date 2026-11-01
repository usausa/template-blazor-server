namespace Template.BlazorServer.Host.Components.Pages.Ignite;

using IgniteUI.Blazor.Controls;

public sealed partial class NavigationPage
{
    private const int StepCount = 4;

    private static readonly DrawerItem[] DrawerItems =
    [
        new("Home", "home"),
        new("Mail", "mail"),
        new("Folders", "folder"),
        new("Settings", "settings")
    ];

    private static readonly Contact[] Contacts =
    [
        new("OS", "Olivia Smith", "555-0101"),
        new("LJ", "Liam Johnson", "555-0102"),
        new("EB", "Emma Brown", "555-0103")
    ];

#pragma warning disable CA2213
    private IgbNavDrawer drawer = default!;

    private IgbStepper stepper = default!;
#pragma warning restore CA2213

    private string drawerItem = "Home";

    private string[] treeSelection = [];

    private int stepIndex;

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private Task<bool> OnDrawerToggleAsync() => drawer.ToggleAsync();

    private void OnDrawerItemClick(string name) => drawerItem = name;

    private void OnTreeSelectionChanged(IgbTreeSelectionEventArgs args) =>
        treeSelection = [.. args.Detail.NewSelection.Select(static x => x.Label ?? string.Empty)];

    private void OnActiveStepChanged(IgbActiveStepChangedEventArgs args) => stepIndex = (int)args.Detail.Index;

    private async Task OnPreviousStepAsync()
    {
        await stepper.PrevAsync();
        stepIndex = Math.Max(stepIndex - 1, 0);
    }

    private async Task OnNextStepAsync()
    {
        await stepper.NextAsync();
        stepIndex = Math.Min(stepIndex + 1, StepCount - 1);
    }
}

internal sealed record DrawerItem(string Name, string Icon);

internal sealed record Contact(string Initials, string Name, string Phone);
