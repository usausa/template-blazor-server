namespace Template.BlazorServer.Models.Entity;

public sealed class CalendarEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    public string Color { get; set; } = default!;

    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }
}
