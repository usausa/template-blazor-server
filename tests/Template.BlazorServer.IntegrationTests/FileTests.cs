namespace Template.BlazorServer;

using System.Net.Http.Headers;
using System.Net.Http.Json;

using Template.BlazorServer.Host.Endpoints;

public sealed class FileTests : IClassFixture<TestApplicationFactory>
{
    private static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    private readonly TestApplicationFactory factory;

    public FileTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task ListReturnsTypedEntries()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        await UploadAsync(client, "list.png", Png);

        // Act
        var response = await client.GetFromJsonAsync<FileListResponse>(new Uri("/api/files/list/", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        var entry = Assert.Single(response.Entries, static x => x.Name == "list.png");
        Assert.False(entry.IsDirectory);
        Assert.Equal(Png.Length, entry.Size);
        Assert.Equal(TimeSpan.Zero, entry.LastModified.Offset);
    }

    [Fact]
    public async Task PreviewReturnsInlineContentWithEntityTag()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        await UploadAsync(client, "preview.png", Png);
        var uri = new Uri("/api/files/preview/preview.png", UriKind.Relative);

        // Act
        var response = await client.GetAsync(uri, TestContext.Current.CancellationToken);

        using var cached = new HttpRequestMessage(HttpMethod.Get, uri);
        cached.Headers.IfNoneMatch.Add(response.Headers.ETag!);
        var notModified = await client.SendAsync(cached, TestContext.Current.CancellationToken);

        using var partial = new HttpRequestMessage(HttpMethod.Get, uri);
        partial.Headers.Range = new RangeHeaderValue(0, 9);
        var ranged = await client.SendAsync(partial, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.Null(response.Content.Headers.ContentDisposition);
        Assert.Equal(HttpStatusCode.NotModified, notModified.StatusCode);
        Assert.Equal(HttpStatusCode.PartialContent, ranged.StatusCode);
        Assert.Equal(10, (await ranged.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Length);
    }

    [Theory]
    [InlineData("payload.svg")]
    [InlineData("payload.html")]
    [InlineData("payload.zip")]
    public async Task PreviewRejectsUnsupportedType(string name)
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        await UploadAsync(client, name, "<svg xmlns=\"http://www.w3.org/2000/svg\"/>"u8.ToArray());

        // Act
        var response = await client.GetAsync(new Uri($"/api/files/preview/{name}", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task UploadAsync(HttpClient client, string name, byte[] data)
    {
        using var content = new MultipartFormDataContent();
        using var file = new ByteArrayContent(data);
        content.Add(file, "files", name);
        var response = await client.PostAsync(new Uri("/api/files/upload/", UriKind.Relative), content, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
