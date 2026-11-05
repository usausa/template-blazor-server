namespace Template.BlazorServer;

using Microsoft.AspNetCore.Mvc.Testing;

public sealed class CultureTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public CultureTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Theory]
    [InlineData("ja-JP")]
    [InlineData("en-US")]
    public async Task ChangeSavesCultureToCookie(string name)
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Act
        var response = await client.GetAsync(new Uri($"/culture/{name}", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/textresource/locale", response.Headers.Location?.OriginalString);
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            x => x.StartsWith($".AspNetCore.Culture=c%3D{name}%7Cuic%3D{name}", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ChangeUnknownCultureReturnsNotFound()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Act
        var response = await client.GetAsync(new Uri("/culture/fr-FR", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie") && response.Headers.GetValues("Set-Cookie").Any(static x => x.StartsWith(".AspNetCore.Culture=", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task ChangeWithoutAuthRedirectsToLogin()
    {
        // Arrange
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Act
        var response = await client.GetAsync(new Uri("/culture/en-US", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }
}
