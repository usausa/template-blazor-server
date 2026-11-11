namespace Template.BlazorServer.Models.Entity;

public sealed class CalendarEventEntity
{
    public long Id { get; set; }

    public long CalendarId { get; set; }

    public string Title { get; set; } = default!;

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public bool AllDay { get; set; }

    public string? Description { get; set; }

    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }
}
