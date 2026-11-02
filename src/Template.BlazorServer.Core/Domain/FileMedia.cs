namespace Template.BlazorServer.Domain;

public static class FileMedia
{
    private const string TextContentType = "text/plain; charset=utf-8";

    private static readonly Dictionary<string, (FileKind Kind, string ContentType)> Types = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = (FileKind.Image, "image/jpeg"),
        [".jpeg"] = (FileKind.Image, "image/jpeg"),
        [".png"] = (FileKind.Image, "image/png"),
        [".gif"] = (FileKind.Image, "image/gif"),
        [".bmp"] = (FileKind.Image, "image/bmp"),
        [".webp"] = (FileKind.Image, "image/webp"),
        [".mp4"] = (FileKind.Video, "video/mp4"),
        [".webm"] = (FileKind.Video, "video/webm"),
        [".mp3"] = (FileKind.Audio, "audio/mpeg"),
        [".wav"] = (FileKind.Audio, "audio/wav"),
        [".ogg"] = (FileKind.Audio, "audio/ogg"),
        [".txt"] = (FileKind.Text, TextContentType),
        [".log"] = (FileKind.Text, TextContentType),
        [".md"] = (FileKind.Text, TextContentType),
        [".csv"] = (FileKind.Text, TextContentType),
        [".json"] = (FileKind.Text, TextContentType),
        [".xml"] = (FileKind.Text, TextContentType),
        [".pdf"] = (FileKind.Pdf, "application/pdf")
    };

    public static FileKind GetKind(string name) =>
        Types.TryGetValue(Path.GetExtension(name), out var type) ? type.Kind : FileKind.Other;

    public static string? GetContentType(string name) =>
        Types.TryGetValue(Path.GetExtension(name), out var type) ? type.ContentType : null;
}
