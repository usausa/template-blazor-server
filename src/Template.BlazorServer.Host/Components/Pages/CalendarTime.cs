namespace Template.BlazorServer.Host.Components.Pages;

public static class CalendarTime
{
    private static readonly TimeSpan GapStep = TimeSpan.FromMinutes(30);

    public static TimeZoneInfo FindZone(string? id) =>
        !String.IsNullOrEmpty(id) && TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone) ? zone : TimeZoneInfo.Local;

    public static DateTime ToLocal(DateTime value, bool allDay, TimeZoneInfo zone) =>
        allDay
            ? DateTime.SpecifyKind(value, DateTimeKind.Unspecified)
            : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(value, DateTimeKind.Utc), zone);

    public static DateTime ToStore(DateTime local, bool allDay, TimeZoneInfo zone) =>
        allDay ? DateTime.SpecifyKind(local.Date, DateTimeKind.Unspecified) : ToUtc(local, zone);

    public static DateTime ToUtc(DateTime local, TimeZoneInfo zone)
    {
        var value = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        while (zone.IsInvalidTime(value))
        {
            value = value.Add(GapStep);
        }

        return TimeZoneInfo.ConvertTimeToUtc(value, zone);
    }
}
