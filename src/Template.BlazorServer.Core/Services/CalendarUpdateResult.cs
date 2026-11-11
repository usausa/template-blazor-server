namespace Template.BlazorServer.Services;

public readonly record struct CalendarUpdateResult(CalendarWriteStatus Status, int Version);
