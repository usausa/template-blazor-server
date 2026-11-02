namespace Template.BlazorServer;

using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

public sealed partial class ReportTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public ReportTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [GeneratedRegex(@"/Type\s*/Page\b")]
    private static partial Regex PdfPageRegex();

    [GeneratedRegex(@"/Subtype\s*/Image\b")]
    private static partial Regex PdfImageRegex();

    [Fact]
    public async Task ProductListReturnsInlinePdfPerPage()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync(new Uri("/api/reports/product-list?rows=45", UriKind.Relative), TestContext.Current.CancellationToken);
        var bytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("inline", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("product-list.pdf", response.Content.Headers.ContentDisposition?.FileName);
        Assert.Equal(3, CountPages(bytes));
    }

    [Fact]
    public async Task DownloadReturnsAttachment()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync(new Uri("/api/reports/product-list?rows=5&download=true", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("product-list.pdf", response.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task ProductListWithInvalidRowsReturnsValidationProblem()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync(new Uri("/api/reports/product-list?rows=0", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("/api/reports/slip?sheets=receipt", 1)]
    [InlineData("/api/reports/slip?sheets=delivery&sheets=invoice&note=memo", 2)]
    [InlineData("/api/reports/slip", 3)]
    public async Task SlipReturnsSelectedSheets(string path, int pages)
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync(new Uri(path, UriKind.Relative), TestContext.Current.CancellationToken);
        var bytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(pages, CountPages(bytes));
    }

    [Fact]
    public async Task SlipHasQrCodeImage()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();

        // Act
        var slip = await client.GetByteArrayAsync(new Uri("/api/reports/slip?sheets=receipt", UriKind.Relative), TestContext.Current.CancellationToken);
        var productList = await client.GetByteArrayAsync(new Uri("/api/reports/product-list?rows=5", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, CountImages(slip));
        Assert.Equal(0, CountImages(productList));
    }

    [Fact]
    public async Task SlipWithUnknownSheetReturnsBadRequest()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync(new Uri("/api/reports/slip?sheets=unknown", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvoiceWithoutDataReturnsBadRequest()
    {
        // Arrange
        await using var emptyFactory = new TestApplicationFactory();
        var client = await emptyFactory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync(new Uri("/api/reports/invoice", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvoicePagesEvery25Rows()
    {
        // Arrange
        await using var dataFactory = new TestApplicationFactory();
        var client = await dataFactory.CreateAuthenticatedClientAsync();
        for (var i = 1; i <= 26; i++)
        {
            var created = await client.PostAsJsonAsync(new Uri("/api/data/", UriKind.Relative), new { name = $"Item {i}", value = i * 100 }, TestContext.Current.CancellationToken);
            created.EnsureSuccessStatusCode();
        }

        // Act
        var response = await client.GetAsync(new Uri("/api/reports/invoice", UriKind.Relative), TestContext.Current.CancellationToken);
        var bytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("inline", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal(2, CountPages(bytes));
    }

    private static int CountPages(byte[] pdf) => PdfPageRegex().Count(Encoding.Latin1.GetString(pdf));

    private static int CountImages(byte[] pdf) => PdfImageRegex().Count(Encoding.Latin1.GetString(pdf));
}
