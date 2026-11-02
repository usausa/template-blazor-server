namespace Template.BlazorServer.Services;

public sealed class NotificationChangedEventArgs : EventArgs
{
    public string? UserName { get; }

    public NotificationChangedEventArgs(string? userName)
    {
        UserName = userName;
    }
}
