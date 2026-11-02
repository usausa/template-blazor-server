namespace Template.BlazorServer.Domain;

public sealed class FileMediaTests
{
    [Theory]
    [InlineData("photo.jpg", FileKind.Image, "image/jpeg")]
    [InlineData("PHOTO.PNG", FileKind.Image, "image/png")]
    [InlineData("movie.mp4", FileKind.Video, "video/mp4")]
    [InlineData("sound.mp3", FileKind.Audio, "audio/mpeg")]
    [InlineData("note.txt", FileKind.Text, "text/plain; charset=utf-8")]
    [InlineData("report.pdf", FileKind.Pdf, "application/pdf")]
    public void PreviewableFileHasKindAndContentType(string name, FileKind kind, string contentType)
    {
        // Act
        var actualKind = FileMedia.GetKind(name);
        var actualContentType = FileMedia.GetContentType(name);

        // Assert
        Assert.Equal(kind, actualKind);
        Assert.Equal(contentType, actualContentType);
    }

    [Theory]
    [InlineData("image.svg")]
    [InlineData("page.html")]
    [InlineData("script.js")]
    [InlineData("archive.zip")]
    [InlineData("noextension")]
    public void OtherFileHasNoContentType(string name)
    {
        // Act
        var kind = FileMedia.GetKind(name);
        var contentType = FileMedia.GetContentType(name);

        // Assert
        Assert.Equal(FileKind.Other, kind);
        Assert.Null(contentType);
    }
}
