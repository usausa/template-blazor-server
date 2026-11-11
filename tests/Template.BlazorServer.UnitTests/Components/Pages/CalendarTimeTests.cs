namespace Template.BlazorServer.Components.Pages;

using Template.BlazorServer.Host.Components.Pages;

public sealed class CalendarTimeTests
{
    private static readonly TimeZoneInfo Tokyo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");

    private static readonly TimeZoneInfo NewYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    [Fact]
    public void TimedValueIsStoredInUtc()
    {
        // Act
        var stored = CalendarTime.ToStore(new DateTime(2026, 11, 9, 10, 0, 0), false, Tokyo);
        var local = CalendarTime.ToLocal(stored, false, Tokyo);

        // Assert
        Assert.Equal(new DateTime(2026, 11, 9, 1, 0, 0), stored);
        Assert.Equal(DateTimeKind.Utc, stored.Kind);
        Assert.Equal(new DateTime(2026, 11, 9, 10, 0, 0), local);
    }

    [Fact]
    public void AllDayValueKeepsDateInEveryZone()
    {
        // Act
        var stored = CalendarTime.ToStore(new DateTime(2026, 11, 9, 13, 30, 0), true, Tokyo);
        var local = CalendarTime.ToLocal(DateTime.SpecifyKind(stored, DateTimeKind.Utc), true, NewYork);

        // Assert
        Assert.Equal(new DateTime(2026, 11, 9), stored);
        Assert.Equal(new DateTime(2026, 11, 9), local);
    }

    [Fact]
    public void SkippedLocalTimeMovesToNextValidTime()
    {
        // Act
        var utc = CalendarTime.ToUtc(new DateTime(2026, 3, 8, 2, 30, 0), NewYork);

        // Assert
        Assert.Equal(new DateTime(2026, 3, 8, 7, 0, 0), utc);
    }

    [Fact]
    public void FindZoneFallsBackToLocal()
    {
        // Act
        var found = CalendarTime.FindZone("Asia/Tokyo");
        var unknown = CalendarTime.FindZone("Unknown/Zone");
        var empty = CalendarTime.FindZone(null);

        // Assert
        Assert.Equal(TimeSpan.FromHours(9), found.BaseUtcOffset);
        Assert.Equal(TimeZoneInfo.Local.Id, unknown.Id);
        Assert.Equal(TimeZoneInfo.Local.Id, empty.Id);
    }
}
