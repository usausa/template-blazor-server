namespace Template.BlazorServer.Host.Application.Urls;

using Template.BlazorServer.Host.Endpoints;

public static class SampleUrls
{
    public static string Upload => $"{ApiRoutes.Samples.TrimStart('/')}/upload";
}
