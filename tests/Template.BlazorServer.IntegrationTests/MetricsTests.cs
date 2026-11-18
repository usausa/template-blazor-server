namespace Template.BlazorServer;

using System.Diagnostics.Metrics;

using Template.BlazorServer.Host.Application.Telemetry;

public sealed class MetricsTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    private long count;

    public MetricsTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task ApiRequestIsCounted()
    {
        // Arrange
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if ((instrument.Meter.Name == Source.Name) && (instrument.Name == "api.request.execution"))
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, measurement, _, _) => Interlocked.Add(ref count, measurement));
        listener.Start();

        var client = await factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync(new Uri("/api/data", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(Interlocked.Read(ref count) >= 1);
    }
}
