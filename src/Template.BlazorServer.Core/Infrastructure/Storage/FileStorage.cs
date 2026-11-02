namespace Template.BlazorServer.Infrastructure.Storage;

public sealed class FileStorage : IStorage
{
    private const int CopyBufferSize = 81920;

    private readonly string root;

    public FileStorage(FileStorageOption options)
    {
        root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.Root));
    }

    private string NormalizePath(string path)
    {
        var fullPath = Path.GetFullPath(Path.Combine(root, path));
        if ((fullPath != root) && !fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new StorageException("Invalid path.");
        }

        var current = root;
        foreach (var segment in fullPath[root.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            if (!File.Exists(current) && !Directory.Exists(current))
            {
                break;
            }

            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
            {
                throw new StorageException("Invalid path.");
            }
        }

        return fullPath;
    }

    public ValueTask<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        path = NormalizePath(path);
        return ValueTask.FromResult(File.Exists(path));
    }

    public ValueTask<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        path = NormalizePath(path);
        return ValueTask.FromResult(Directory.Exists(path));
    }

    public ValueTask<StorageEntry[]> ListAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directory = new DirectoryInfo(NormalizePath(path));
        var entries = directory.EnumerateDirectories().Select(static x => new StorageEntry(x.Name, true, 0, x.LastWriteTimeUtc))
            .Concat(directory.EnumerateFiles().Select(static x => new StorageEntry(x.Name, false, x.Length, x.LastWriteTimeUtc)))
            .ToArray();
        return ValueTask.FromResult(entries);
    }

    public ValueTask<StorageEntry?> GetEntryAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        path = NormalizePath(path);
        var file = new FileInfo(path);
        if (file.Exists)
        {
            return ValueTask.FromResult<StorageEntry?>(new StorageEntry(file.Name, false, file.Length, file.LastWriteTimeUtc));
        }

        var directory = new DirectoryInfo(path);
        return ValueTask.FromResult(directory.Exists ? new StorageEntry(directory.Name, true, 0, directory.LastWriteTimeUtc) : null);
    }

    public ValueTask DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        path = NormalizePath(path);
        File.Delete(path);

        return ValueTask.CompletedTask;
    }

    public ValueTask<Stream> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        path = NormalizePath(path);
#pragma warning disable CA2000
        return ValueTask.FromResult((Stream)File.OpenRead(path));
#pragma warning restore CA2000
    }

    public async ValueTask WriteAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        path = NormalizePath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var fs = File.Create(tempPath))
            {
                await stream.CopyToAsync(fs, CopyBufferSize, cancellationToken);
            }

            File.Move(tempPath, path, overwrite: true);
        }
        catch
        {
            File.Delete(tempPath);
            throw;
        }
    }
}
