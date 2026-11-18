namespace Template.BlazorServer;

using Microsoft.AspNetCore.Mvc.Testing;

public sealed class AuthTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public AuthTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Theory]
    [InlineData("cross-site", HttpStatusCode.BadRequest)]
    [InlineData("same-site", HttpStatusCode.BadRequest)]
    [InlineData("same-origin", HttpStatusCode.Redirect)]
    public async Task LoginAllowsOnlySameOrigin(string site, HttpStatusCode expected)
    {
        // Arrange
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["name"] = "admin",
            ["password"] = "admin"
        });
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/auth/login", UriKind.Relative));
        request.Content = form;
        request.Headers.Add("Sec-Fetch-Site", site);

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("cross-site", HttpStatusCode.BadRequest)]
    [InlineData("same-site", HttpStatusCode.BadRequest)]
    [InlineData("same-origin", HttpStatusCode.Redirect)]
    public async Task LogoutAllowsOnlySameOrigin(string site, HttpStatusCode expected)
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/auth/logout", UriKind.Relative));
        request.Headers.Add("Sec-Fetch-Site", site);

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }
}
