namespace Template.BlazorServer.Host.Infrastructure.Reports;

using OysterReport;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Report render warning. kind=[{kind}], sheet=[{sheet}], source=[{source}], message=[{message}]")]
    public static partial void WarnRenderWarning(this ILogger logger, ReportRenderWarningKind kind, string sheet, string? source, string message);
}
