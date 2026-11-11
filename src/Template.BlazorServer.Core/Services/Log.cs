namespace Template.BlazorServer.Services;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Data change handler failed.")]
    public static partial void ErrorDataChangeHandlerFailed(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Notification change handler failed.")]
    public static partial void ErrorNotificationChangeHandlerFailed(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Calendar change handler failed.")]
    public static partial void ErrorCalendarChangeHandlerFailed(this ILogger logger, Exception ex);
}
