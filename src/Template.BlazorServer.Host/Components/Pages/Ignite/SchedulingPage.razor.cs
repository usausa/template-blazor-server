namespace Template.BlazorServer.Host.Components.Pages.Ignite;

using IgniteUI.Blazor.Controls;

public sealed partial class SchedulingPage
{
    private DateTime calendarValue = DateTime.Today;

    private DateTime? date;

    private IgbDateRangeValue<DateTime?>? range;

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static string FormatDate(DateTime? value) =>
        value?.ToLocalTime().ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) ?? string.Empty;
}
