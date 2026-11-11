namespace Template.BlazorServer.Services;

public sealed class CalendarChangeNotifier
{
    private readonly ILogger<CalendarChangeNotifier> log;

    public event EventHandler? Changed;

    public CalendarChangeNotifier(ILogger<CalendarChangeNotifier> log)
    {
        this.log = log;
    }

    public void Notify()
    {
        var handlers = Changed;
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<EventHandler>())
        {
            try
            {
                handler(this, EventArgs.Empty);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.ErrorCalendarChangeHandlerFailed(ex);
            }
        }
    }
}
