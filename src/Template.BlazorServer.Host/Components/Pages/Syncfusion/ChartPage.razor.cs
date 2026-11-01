namespace Template.BlazorServer.Host.Components.Pages.Syncfusion;

public sealed partial class ChartPage
{
    private static readonly string[] Palette = ["#1f60c4", "#5794f2", "#c0d8ff"];

    private static readonly Sale[] Sales =
    [
        new("Jan", 35, 28, 12),
        new("Feb", 28, 35, 15),
        new("Mar", 34, 32, 14),
        new("Apr", 32, 44, 18),
        new("May", 40, 32, 16),
        new("Jun", 32, 35, 20),
        new("Jul", 35, 41, 19),
        new("Aug", 55, 48, 22),
        new("Sep", 38, 52, 21),
        new("Oct", 30, 34, 17),
        new("Nov", 25, 36, 15),
        new("Dec", 32, 40, 18)
    ];

    private static readonly Measurement[] Measurements =
    [
        new(152, 48), new(155, 52), new(158, 50), new(160, 55), new(162, 58),
        new(165, 54), new(167, 61), new(170, 63), new(172, 60), new(174, 68),
        new(176, 70), new(178, 66), new(180, 74), new(182, 77), new(185, 80)
    ];
}

internal sealed record Sale(string Month, double Online, double Retail, double Wholesale);

internal sealed record Measurement(double Height, double Weight);
