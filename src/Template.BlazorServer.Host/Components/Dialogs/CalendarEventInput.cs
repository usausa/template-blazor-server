namespace Template.BlazorServer.Host.Components.Dialogs;

public sealed record CalendarEventInput(string Title, long CalendarId, DateTime Start, DateTime End, bool AllDay, string? Description);
