namespace Template.BlazorServer.Host.Endpoints;

using Template.BlazorServer.Host.Settings;

//--------------------------------------------------------------------------------
// Models
//--------------------------------------------------------------------------------

public sealed class SampleUploadResponse
{
    public string Path { get; set; } = default!;

    public long Size { get; set; }
}

//--------------------------------------------------------------------------------
// Endpoints
//--------------------------------------------------------------------------------

public static class SampleEndpoints
{
    private static readonly char[] InvalidNameChars = Path.GetInvalidFileNameChars();

    //--------------------------------------------------------------------------------
    // Mapping
    //--------------------------------------------------------------------------------

    public static void MapSampleEndpoints(this WebApplication app)
    {
        var setting = app.Services.GetRequiredService<SampleSetting>();

        var group = app.MapApiGroup(ApiRoutes.Samples)
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/upload", HandleUpload)
            .WithName("SampleUpload")
            .Produces<SampleUploadResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithMetadata(new RequestSizeLimitAttribute(setting.UploadLimit))
            .WithFormOptions(multipartBodyLengthLimit: setting.UploadLimit)
            .WithRequestTimeout(TimeSpan.FromMinutes(10));
    }

    //--------------------------------------------------------------------------------
    // Upload
    //--------------------------------------------------------------------------------

    private static IResult HandleUpload(
        [FromForm] string relativePath,
        IFormFile files)
    {
        if (!IsValidRelativePath(relativePath))
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid path.");
        }

        return TypedResults.Ok(new SampleUploadResponse { Path = relativePath, Size = files.Length });
    }

    private static bool IsValidRelativePath(string path) =>
        !String.IsNullOrEmpty(path) &&
        !Path.IsPathRooted(path) &&
        path.Split('/').All(static x => (x.Length > 0) && (x != ".") && (x != "..") && (x.IndexOfAny(InvalidNameChars) < 0));
}
