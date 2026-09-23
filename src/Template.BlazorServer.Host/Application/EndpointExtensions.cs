namespace Template.BlazorServer.Host.Application;

using Template.BlazorServer.Host.Application.Context;
using Template.BlazorServer.Host.Application.Telemetry;

public static class EndpointExtensions
{
    public static RouteGroupBuilder MapApiGroup(this IEndpointRouteBuilder endpoints, string prefix) =>
        endpoints.MapGroup(prefix)
            .AddEndpointFilter<RequestMetricsEndpointFilter>()
            .AddEndpointFilter<ServiceContextEndpointFilter>();
}
