namespace Template.BlazorServer;

using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public sealed class BlazorOptionsTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public BlazorOptionsTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public void CircuitAndHubUseSettings()
    {
        // Act
        var circuit = factory.Services.GetRequiredService<IOptions<CircuitOptions>>().Value;
        var hub = ResolveHubOptions(factory.Services);

        // Assert
        Assert.Equal(100, circuit.DisconnectedCircuitMaxRetained);
        Assert.Equal(TimeSpan.FromMinutes(3), circuit.DisconnectedCircuitRetentionPeriod);
        Assert.Equal(TimeSpan.FromMinutes(1), circuit.JSInteropDefaultCallTimeout);
        Assert.Equal(10, circuit.MaxBufferedUnacknowledgedRenderBatches);
        Assert.Equal(TimeSpan.FromSeconds(30), hub.ClientTimeoutInterval);
        Assert.Equal(TimeSpan.FromSeconds(15), hub.KeepAliveInterval);
        Assert.Equal(TimeSpan.FromSeconds(15), hub.HandshakeTimeout);
        Assert.Equal(32768, hub.MaximumReceiveMessageSize);
        Assert.Equal(1, hub.MaximumParallelInvocationsPerClient);
    }

    [Fact]
    public void SettingsChangeCircuitAndHub()
    {
        // Arrange
        using var changed = factory.WithWebHostBuilder(static builder =>
        {
            builder.UseSetting("Blazor:Circuit:DisconnectedCircuitRetentionPeriod", "00:10:00");
            builder.UseSetting("Blazor:Hub:MaximumReceiveMessageSize", "65536");
        });

        // Act
        var circuit = changed.Services.GetRequiredService<IOptions<CircuitOptions>>().Value;
        var hub = ResolveHubOptions(changed.Services);

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(10), circuit.DisconnectedCircuitRetentionPeriod);
        Assert.Equal(65536, hub.MaximumReceiveMessageSize);
    }

    private static HubOptions ResolveHubOptions(IServiceProvider services)
    {
        var hubType = typeof(CircuitOptions).Assembly.GetType("Microsoft.AspNetCore.Components.Server.ComponentHub", throwOnError: true)!;
        var optionsType = typeof(IOptions<>).MakeGenericType(typeof(HubOptions<>).MakeGenericType(hubType));
        var options = services.GetRequiredService(optionsType);
        return (HubOptions)optionsType.GetProperty(nameof(IOptions<>.Value))!.GetValue(options)!;
    }
}
