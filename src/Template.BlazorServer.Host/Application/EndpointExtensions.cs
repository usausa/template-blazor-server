namespace Template.BlazorServer.Host.Application;

using Template.BlazorServer.Host.Infrastructure.Filters;

public static class EndpointExtensions
{
    public static RouteGroupBuilder MapApiGroup(this IEndpointRouteBuilder endpoints, string prefix) =>
        endpoints.MapGroup(prefix).AddEndpointFilter<RequestMetricsEndpointFilter>();
}
