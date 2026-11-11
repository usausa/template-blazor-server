namespace Template.BlazorServer.Domain;

public static class CalendarColors
{
    public const string Blue = "#3274D9";

    public const string Green = "#56A64B";

    public const string Red = "#E02F44";

    public const string Orange = "#FF780A";

    public const string Yellow = "#F2CC0C";

    public const string Purple = "#A352CC";

    public static IReadOnlyList<string> All { get; } = [Blue, Green, Red, Orange, Yellow, Purple];
}
