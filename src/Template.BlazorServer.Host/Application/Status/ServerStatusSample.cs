namespace Template.BlazorServer.Host.Application.Status;

public sealed record ServerStatusSample(
    DateTimeOffset Time,
    double CpuPercent,
    long WorkingSet,
    long HeapSize,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections,
    int ThreadCount);
