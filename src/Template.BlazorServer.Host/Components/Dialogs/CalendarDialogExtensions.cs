namespace Template.BlazorServer.Host.Components.Dialogs;

using MudBlazor;

public static class CalendarDialogExtensions
{
    public static async ValueTask<CalendarEventInput?> ShowCalendarEventDialog(this IDialogService dialog, string title, CalendarEventInput input, IReadOnlyList<CalendarEntity> calendars)
    {
        var reference = await dialog.ShowAsync<CalendarEventDialog>(
            string.Empty,
            new DialogParameters
            {
                { nameof(CalendarEventDialog.Title), title },
                { nameof(CalendarEventDialog.Input), input },
                { nameof(CalendarEventDialog.Calendars), calendars }
            });
        var result = await reference.Result;
        return (result is { Canceled: false }) ? (CalendarEventInput)result.Data! : null;
    }

    public static async ValueTask<CalendarEntity?> ShowCalendarEditDialog(this IDialogService dialog, string title, CalendarEntity? entity)
    {
        var reference = await dialog.ShowAsync<CalendarEditDialog>(
            string.Empty,
            new DialogParameters
            {
                { nameof(CalendarEditDialog.Title), title },
                { nameof(CalendarEditDialog.Entity), entity }
            });
        var result = await reference.Result;
        return (result is { Canceled: false }) ? (CalendarEntity)result.Data! : null;
    }
}
