namespace Template.BlazorServer.Host.Components.Pages.MusselChart;

using global::MusselChart;

public sealed partial class ChartsPage
{
    private static readonly DateTime Start = new(2026, 7, 1, 0, 0, 0, DateTimeKind.Unspecified);

    private static readonly ChartThreshold[] TemperatureThresholds =
    [
        new() { Level = ChartThresholdLevel.Good },
        new() { Value = 28, Level = ChartThresholdLevel.Warning, Label = "28 °C" },
        new() { Value = 32, Level = ChartThresholdLevel.Critical, Label = "32 °C" }
    ];

    private static readonly WeatherPoint[] Weather = Enumerable.Range(0, 48)
        .Select(static i => new WeatherPoint(
            Start.AddHours(i),
            24 + (7 * Math.Sin((i - 9) * Math.PI / 12)) + ((i % 5) * 0.3),
            23 + (8 * Math.Sin((i - 8) * Math.PI / 12)) - ((i % 3) * 0.4),
            65 - (20 * Math.Sin((i - 9) * Math.PI / 12))))
        .ToArray();

    private static readonly ValuePoint[] Profits = Enumerable.Range(0, 14)
        .Select(static i => new ValuePoint(Start.AddDays(i), Math.Round((Math.Sin(i * 0.9) * 120) + (i * 8) - 30)))
        .ToArray();

    private static readonly ValuePoint[] Empty = [];
}

internal sealed record WeatherPoint(DateTime Time, double Tokyo, double Osaka, double Humidity);

internal sealed record ValuePoint(DateTime Time, double Value);
