namespace Template.BlazorServer;

using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

public sealed partial class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string databaseFile = $"test-{Guid.NewGuid():N}.db";

    private readonly string storageRoot = Path.Combine(Path.GetTempPath(), "template-blazor-server-tests", Guid.NewGuid().ToString("N"));

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryTokenRegex();

    public Task<HttpClient> CreateAuthenticatedClientAsync() =>
        CreateAuthenticatedClientAsync(new WebApplicationFactoryClientOptions());

    public async Task<HttpClient> CreateAuthenticatedClientAsync(WebApplicationFactoryClientOptions options)
    {
        var client = CreateClient(options);
        var loginPage = await client.GetStringAsync(new Uri("/login", UriKind.Relative), TestContext.Current.CancellationToken);
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["name"] = "admin",
            ["password"] = "admin",
            ["__RequestVerificationToken"] = AntiforgeryTokenRegex().Match(loginPage).Groups[1].Value
        });
        await client.PostAsync(new Uri("/auth/login", UriKind.Relative), form, TestContext.Current.CancellationToken);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("http_ports", string.Empty);
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={databaseFile};Pooling=False;Foreign Keys=True");
        builder.UseSetting("Storage:Root", storageRoot);
        builder.UseSetting("Prometheus:Uri", string.Empty);
        builder.UseSetting("Notification:Enable", "false");
        builder.UseSetting("NotificationRetention:Enable", "false");
        builder.UseSetting("Profiler:SqlLog:Enable", "false");
        builder.UseSetting("Profiler:SqlTelemetry:Enable", "false");
        builder.UseSetting("Log:HttpLog", "false");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(databaseFile))
        {
            try
            {
                File.Delete(databaseFile);
            }
            catch (IOException)
            {
                // Ignore
            }
        }

        if (disposing && Directory.Exists(storageRoot))
        {
            try
            {
                Directory.Delete(storageRoot, true);
            }
            catch (IOException)
            {
                // Ignore
            }
        }
    }
}
