namespace Template.BlazorServer.Infrastructure.Storage;

public sealed record StorageEntry(string Name, bool IsDirectory, long Size, DateTimeOffset LastModified);
