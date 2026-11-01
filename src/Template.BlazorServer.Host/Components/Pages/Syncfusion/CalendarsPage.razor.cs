namespace Template.BlazorServer.Host.Components.Pages.Syncfusion;

public sealed partial class CalendarsPage
{
    private DateTime calendarValue = DateTime.Today;

    private DateTime? date;

    private DateTime? dateTime;

    private DateTime? time;

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static string Format(DateTime? value, string format) =>
        value?.ToString(format, CultureInfo.InvariantCulture) ?? string.Empty;
}
