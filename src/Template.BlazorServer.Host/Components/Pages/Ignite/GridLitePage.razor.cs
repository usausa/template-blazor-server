namespace Template.BlazorServer.Host.Components.Pages.Ignite;

using IgniteUI.Blazor.Controls;

public sealed partial class GridLitePage
{
    private static readonly Employee[] Employees =
    [
        new(1, "Olivia Smith", "Sales", 5200, false),
        new(2, "Liam Johnson", "Development", 6800, true),
        new(3, "Emma Brown", "Support", 4500, true),
        new(4, "Noah Davis", "Development", 7100, false),
        new(5, "Ava Wilson", "Sales", 4900, true),
        new(6, "Lucas Moore", "Marketing", 5600, false),
        new(7, "Mia Taylor", "Development", 6300, true),
        new(8, "Ethan Anderson", "Support", 4300, false),
        new(9, "Sophia Thomas", "Marketing", 5900, true),
        new(10, "Mason Jackson", "Sales", 5100, false),
        new(11, "Isabella White", "Development", 7400, true),
        new(12, "James Harris", "Support", 4700, false)
    ];

    private string sorted = "-";

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnSorted(IgbGridLiteSortedEventArgs args) => sorted = $"{args.Expression.Key} {args.Expression.Direction}";
}

internal sealed record Employee(int Id, string Name, string Department, int Salary, bool Remote);
