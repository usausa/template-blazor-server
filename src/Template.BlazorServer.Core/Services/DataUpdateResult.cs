namespace Template.BlazorServer.Services;

public readonly record struct DataUpdateResult(DataWriteStatus Status, int Version);
