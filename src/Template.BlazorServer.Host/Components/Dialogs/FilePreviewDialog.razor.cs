namespace Template.BlazorServer.Host.Components.Dialogs;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Template.BlazorServer.Host.Application.Urls;
using Template.BlazorServer.Infrastructure.Storage;

public sealed partial class FilePreviewDialog
{
    private const int TextLimit = 100 * 1024;

    private string text = string.Empty;

    private bool truncated;

    //--------------------------------------------------------------------------------
    // Parameter
    //--------------------------------------------------------------------------------

    [Parameter]
    public required StorageEntry Entry { get; set; }

    [CascadingParameter]
    public required IMudDialogInstance MudDialog { get; set; }

    [Inject]
    public required IStorage Storage { get; set; }

    private FileKind Kind => FileMedia.GetKind(Entry.Name);

    private bool IsImage => Kind == FileKind.Image;

    private bool IsVideo => Kind == FileKind.Video;

    private bool IsAudio => Kind == FileKind.Audio;

    private bool IsText => Kind == FileKind.Text;

    private string PreviewUrl => FileUrls.Preview(Entry.Name, Entry.LastModified);

    private string DownloadUrl => FileUrls.Download(Entry.Name);

    //--------------------------------------------------------------------------------
    // Events
    //--------------------------------------------------------------------------------

    protected override async Task OnInitializedAsync()
    {
        if (!IsText)
        {
            return;
        }

        await using var stream = await Storage.ReadAsync(Entry.Name);
        var buffer = new byte[TextLimit];
        var length = await stream.ReadAtLeastAsync(buffer, TextLimit, throwOnEndOfStream: false);
        text = Encoding.UTF8.GetString(buffer, 0, length);
        truncated = Entry.Size > TextLimit;
    }

    private void OnCloseClick() => MudDialog.Close();
}
