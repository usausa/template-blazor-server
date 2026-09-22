namespace Template.BlazorServer.Host.Infrastructure.Circuits;

public sealed record CircuitInfo(string Id, string? User, DateTimeOffset OpenedAt, bool Connected);
