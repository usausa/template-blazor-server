namespace Template.BlazorServer.Host.Components.Pages.Ignite;

using IgniteUI.Blazor.Controls;

public sealed partial class InputsPage
{
    private string? name;

    private string phone = string.Empty;

    private string notes = string.Empty;

    private DateTime? dateTime;

    private bool agreed;

    private string? plan = "basic";

    private bool notifications = true;

    private double volume = 40;

    private double lower = 20;

    private double upper = 80;

    private double rating = 3;

    private string? color = "#0f6cbd";

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnRangeChange(IgbRangeSliderValueEventArgs args)
    {
        lower = args.Detail.Lower;
        upper = args.Detail.Upper;
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static string FormatDateTime(DateTime? value) =>
        value?.ToLocalTime().ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
}
