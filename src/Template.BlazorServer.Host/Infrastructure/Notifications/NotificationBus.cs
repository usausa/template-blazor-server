namespace Template.BlazorServer.Host.Infrastructure.Notifications;

public sealed class NotificationBus
{
    private readonly ILogger<NotificationBus> log;

    public event EventHandler<NotificationEventArgs>? Received;

    public NotificationBus(ILogger<NotificationBus> log)
    {
        this.log = log;
    }

    public void Publish(string message) =>
        Invoke(Received, new NotificationEventArgs(message));

    private void Invoke<T>(EventHandler<T>? handlers, T args)
        where T : EventArgs
    {
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<EventHandler<T>>())
        {
            try
            {
                handler(this, args);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.ErrorNotificationHandlerFailed(ex);
            }
        }
    }
}
