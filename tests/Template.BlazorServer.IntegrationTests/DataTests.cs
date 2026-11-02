namespace Template.BlazorServer;

using System.Net.Http.Json;

using Template.BlazorServer.Host.Endpoints;

public sealed class DataTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public DataTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task UpdateRejectsStaleVersion()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        var create = await client.PostAsJsonAsync(new Uri("/api/data/", UriKind.Relative), new DataCreateRequest { Name = "VersionItem", Value = 1 }, TestContext.Current.CancellationToken);
        var id = (await create.Content.ReadFromJsonAsync<DataCreateResponse>(TestContext.Current.CancellationToken))!.Id;
        var path = new Uri($"/api/data/{id}", UriKind.Relative);

        // Act
        var get = await client.GetAsync(path, TestContext.Current.CancellationToken);
        var etag = get.Headers.ETag!.Tag;

        using var matched = new HttpRequestMessage(HttpMethod.Put, path);
        matched.Headers.IfMatch.ParseAdd(etag);
        matched.Content = JsonContent.Create(new DataUpdateRequest { Name = "VersionItem", Value = 2 });
        var updated = await client.SendAsync(matched, TestContext.Current.CancellationToken);

        using var stale = new HttpRequestMessage(HttpMethod.Put, path);
        stale.Headers.IfMatch.ParseAdd(etag);
        stale.Content = JsonContent.Create(new DataUpdateRequest { Name = "VersionItem", Value = 3 });
        var rejected = await client.SendAsync(stale, TestContext.Current.CancellationToken);

        var unconditional = await client.PutAsJsonAsync(path, new DataUpdateRequest { Name = "VersionItem", Value = 4 }, TestContext.Current.CancellationToken);
        var latest = await client.GetFromJsonAsync<DataGetResponse>(path, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("\"1\"", etag);
        Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);
        Assert.Equal("\"2\"", updated.Headers.ETag!.Tag);
        Assert.Equal(HttpStatusCode.PreconditionFailed, rejected.StatusCode);
        Assert.Equal("application/problem+json", rejected.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.NoContent, unconditional.StatusCode);
        Assert.NotNull(latest);
        Assert.Equal(3, latest.Version);
        Assert.Equal(4, latest.Value);
    }
}
