namespace Template.BlazorServer.Domain;

using System.Text.RegularExpressions;

public static partial class NotificationLink
{
    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9/._~?#=&%+-]*$")]
    private static partial Regex PathRegex();

    public static bool IsValid(string? link) =>
        String.IsNullOrEmpty(link) || CanOpen(link);

    public static bool CanOpen([NotNullWhen(true)] string? link) =>
        !String.IsNullOrEmpty(link) && (link.Length <= Length.NotificationLink) && PathRegex().IsMatch(link);
}
