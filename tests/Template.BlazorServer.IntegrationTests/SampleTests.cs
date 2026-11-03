namespace Template.BlazorServer;

public sealed class SampleTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public SampleTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task UploadRequiresAntiforgeryToken()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        using var content = new MultipartFormDataContent();
        using var path = new StringContent("sample.txt");
        using var file = new ByteArrayContent("sample"u8.ToArray());
        content.Add(path, "relativePath");
        content.Add(file, "files", "sample.txt");

        // Act
        var response = await client.PostAsync(new Uri("/api/samples/upload", UriKind.Relative), content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
