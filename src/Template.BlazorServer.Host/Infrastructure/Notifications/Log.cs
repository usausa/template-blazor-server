namespace Template.BlazorServer.Host.Infrastructure.Notifications;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Notification handler failed.")]
    public static partial void ErrorNotificationHandlerFailed(this ILogger logger, Exception ex);
}
