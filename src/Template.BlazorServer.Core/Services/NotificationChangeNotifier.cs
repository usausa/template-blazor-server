namespace Template.BlazorServer.Services;

public sealed class NotificationChangeNotifier
{
    private readonly ILogger<NotificationChangeNotifier> log;

    public event EventHandler<NotificationChangedEventArgs>? Changed;

    public NotificationChangeNotifier(ILogger<NotificationChangeNotifier> log)
    {
        this.log = log;
    }

    public void Notify(string? userName)
    {
        var handlers = Changed;
        if (handlers is null)
        {
            return;
        }

        var args = new NotificationChangedEventArgs(userName);
        foreach (var handler in handlers.GetInvocationList().Cast<EventHandler<NotificationChangedEventArgs>>())
        {
            try
            {
                handler(this, args);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.ErrorNotificationChangeHandlerFailed(ex);
            }
        }
    }
}
