namespace Template.BlazorServer.Host.Application.Urls;

using Template.BlazorServer.Host.Endpoints;

public static class FileUrls
{
    public static string Download(string name) =>
        $"{ApiRoutes.Files.TrimStart('/')}/download/{Uri.EscapeDataString(name)}";

    public static string Preview(string name, DateTimeOffset lastModified) =>
        $"{ApiRoutes.Files.TrimStart('/')}/preview/{Uri.EscapeDataString(name)}?v={lastModified.UtcTicks}";
}
