namespace Template.BlazorServer.Host.Infrastructure.Reports;

public static class ReportText
{
    public static string Number(long value) => value.ToString("#,##0", CultureInfo.InvariantCulture);

    public static string Date(DateTimeOffset value) => value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

    public static string DateTime(DateTimeOffset value) => value.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
}
