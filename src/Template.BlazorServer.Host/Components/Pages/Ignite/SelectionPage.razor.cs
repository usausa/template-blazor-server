namespace Template.BlazorServer.Host.Components.Pages.Ignite;

using IgniteUI.Blazor.Controls;

public sealed partial class SelectionPage
{
    private static readonly CityItem[] CityItems =
    [
        new("tokyo", "Tokyo", "Japan"),
        new("osaka", "Osaka", "Japan"),
        new("seattle", "Seattle", "United States"),
        new("chicago", "Chicago", "United States"),
        new("london", "London", "United Kingdom")
    ];

    private static readonly CountryItem[] CountryItems =
    [
        new("jp", "Japan"),
        new("us", "United States"),
        new("uk", "United Kingdom"),
        new("de", "Germany")
    ];

    private string[] cities = ["tokyo"];

    private string? country;

    private string? action;

    private string? alignment = "left";

    private List<string> tags = ["Blazor", "Web Components", "MIT"];

    private bool selected;

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnDropdownChange(IgbDropdownItemComponentEventArgs args) => action = args.Detail.Value;

    private void OnAlignmentSelect(IgbComponentValueChangedEventArgs args) => alignment = args.Detail;

    private void OnTagRemove(string tag) => tags = [.. tags.Where(x => x != tag)];
}

internal sealed record CityItem(string Id, string Name, string Country);

internal sealed record CountryItem(string Code, string Name);
