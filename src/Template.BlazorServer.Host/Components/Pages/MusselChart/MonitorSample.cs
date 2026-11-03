namespace Template.BlazorServer.Host.Components.Pages.MusselChart;

public sealed record MonitorSample(
    DateTime Time,
    double Core1,
    double Core2,
    double Core3,
    double Core4,
    double Receive,
    double Send,
    double Queue);
