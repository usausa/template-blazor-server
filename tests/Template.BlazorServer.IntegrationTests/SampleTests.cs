namespace Template.BlazorServer;

public sealed class SampleTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public SampleTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Theory]
    [InlineData("cross-site", HttpStatusCode.BadRequest)]
    [InlineData("same-site", HttpStatusCode.BadRequest)]
    [InlineData("same-origin", HttpStatusCode.OK)]
    public async Task UploadAllowsOnlySameOrigin(string site, HttpStatusCode expected)
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        using var content = new MultipartFormDataContent();
        using var path = new StringContent("sample.txt");
        using var file = new ByteArrayContent("sample"u8.ToArray());
        content.Add(path, "relativePath");
        content.Add(file, "files", "sample.txt");
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/api/samples/upload", UriKind.Relative));
        request.Content = content;
        request.Headers.Add("Sec-Fetch-Site", site);

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }
}
