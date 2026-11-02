namespace Template.BlazorServer.Models.Entity;

public sealed class NotificationEntity
{
    public long Id { get; set; }

    public string UserName { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Body { get; set; } = default!;

    public string? Link { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }
}
