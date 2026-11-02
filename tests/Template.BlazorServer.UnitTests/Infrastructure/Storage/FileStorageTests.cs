namespace Template.BlazorServer.Infrastructure.Storage;

public sealed class FileStorageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "template-storage-tests", Guid.NewGuid().ToString("N"));

    private readonly string outside = Path.Combine(Path.GetTempPath(), "template-storage-tests", Guid.NewGuid().ToString("N"));

    public FileStorageTests()
    {
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(outside);
    }

    public void Dispose()
    {
        Directory.Delete(root, true);
        Directory.Delete(outside, true);
    }

    [Fact]
    public async Task WriteAndReadInsideRoot()
    {
        // Arrange
        var storage = new FileStorage(new FileStorageOption { Root = root });
        await using var source = new MemoryStream("data"u8.ToArray());

        // Act
        await storage.WriteAsync("dir/file.txt", source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(await storage.FileExistsAsync("dir/file.txt", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListReturnsDirectoriesAndFiles()
    {
        // Arrange
        var storage = new FileStorage(new FileStorageOption { Root = root });
        Directory.CreateDirectory(Path.Combine(root, "folder"));
        await File.WriteAllTextAsync(Path.Combine(root, "file.txt"), "data", TestContext.Current.CancellationToken);

        // Act
        var entries = await storage.ListAsync(string.Empty, TestContext.Current.CancellationToken);

        // Assert
        var folder = Assert.Single(entries, static x => x.Name == "folder");
        var file = Assert.Single(entries, static x => x.Name == "file.txt");
        Assert.True(folder.IsDirectory);
        Assert.False(file.IsDirectory);
        Assert.Equal(4, file.Size);
        Assert.Equal(TimeSpan.Zero, file.LastModified.Offset);
    }

    [Fact]
    public async Task GetEntryReturnsNullWhenMissing()
    {
        // Arrange
        var storage = new FileStorage(new FileStorageOption { Root = root });

        // Act
        var entry = await storage.GetEntryAsync("missing.txt", TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(entry);
    }

    [Fact]
    public async Task RejectsPathOutsideRoot()
    {
        // Arrange
        var storage = new FileStorage(new FileStorageOption { Root = root });

        // Act / Assert
        await Assert.ThrowsAsync<StorageException>(async () => await storage.FileExistsAsync("../file.txt", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RejectsPathThroughSymbolicLink()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(outside, "secret.txt"), "secret", TestContext.Current.CancellationToken);
        try
        {
            Directory.CreateSymbolicLink(Path.Combine(root, "link"), outside);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            Assert.Skip("Symbolic links are not available.");
        }

        var storage = new FileStorage(new FileStorageOption { Root = root });

        // Act / Assert
        await Assert.ThrowsAsync<StorageException>(async () => await storage.ReadAsync("link/secret.txt", TestContext.Current.CancellationToken));
    }
}
