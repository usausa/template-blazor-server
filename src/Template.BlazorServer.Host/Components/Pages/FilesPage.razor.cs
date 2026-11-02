namespace Template.BlazorServer.Host.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using MudBlazor;

using Template.BlazorServer.Host.Application.Urls;
using Template.BlazorServer.Host.Components.Dialogs;
using Template.BlazorServer.Host.Components.Shared;
using Template.BlazorServer.Host.Infrastructure.IO;
using Template.BlazorServer.Infrastructure.Storage;

public sealed partial class FilesPage
{
    private const long MaxFileSize = 100L * 1024 * 1024;

    private StorageEntry[] entries = [];

    private ViewMode view = ViewMode.List;

    private bool uploading;

    private int progress;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required IStorage Storage { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    private bool IsListView => view == ViewMode.List;

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override Task OnInitializedAsync() =>
        LoadAsync();

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnProgress(int percent)
    {
        _ = InvokeAsync(() =>
        {
            progress = percent;
            StateHasChanged();
        });
    }

    private Task OnRowClickAsync(DataGridRowClickEventArgs<StorageEntry> args) =>
        CanPreview(args.Item) ? PreviewAsync(args.Item) : Task.CompletedTask;

    //--------------------------------------------------------------------------------
    // Action
    //--------------------------------------------------------------------------------

    private async Task LoadAsync()
    {
        entries = (await Storage.ListAsync(string.Empty))
            .OrderByDescending(static x => x.IsDirectory)
            .ThenBy(static x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private Task OpenAsync(StorageEntry entry) =>
        CanPreview(entry) ? PreviewAsync(entry) : Task.CompletedTask;

    private async Task PreviewAsync(StorageEntry entry)
    {
        await DialogService.ShowFilePreviewDialog(entry);
    }

    private async Task UploadAsync(IBrowserFile? file)
    {
        if (file is null)
        {
            return;
        }

        uploading = true;
        progress = 0;
        try
        {
            var fileName = Path.GetFileName(file.Name);
            await using var browser = file.OpenReadStream(MaxFileSize);
            await using var progressStream = new ReadProgressStream(browser, file.Size, OnProgress);
            await Storage.WriteAsync(fileName, progressStream);

            Snackbar.AddSuccess($"{fileName} をアップロードしました。");
        }
        catch (IOException)
        {
            Snackbar.AddError("アップロードに失敗しました。");
        }
        finally
        {
            uploading = false;
        }

        await LoadAsync();
    }

    private async Task DeleteAsync(StorageEntry entry)
    {
        if (!await DialogService.ShowConfirm("ファイル削除", $"{entry.Name} を削除してよろしいですか？"))
        {
            return;
        }

        await Storage.DeleteAsync(entry.Name);
        Snackbar.AddSuccess("削除しました。");
        await LoadAsync();
    }

    //--------------------------------------------------------------------------------
    // Format
    //--------------------------------------------------------------------------------

    private static bool HasThumbnail(StorageEntry entry) =>
        !entry.IsDirectory && (FileMedia.GetKind(entry.Name) == FileKind.Image);

    private static bool CanPreview(StorageEntry entry) =>
        !entry.IsDirectory && FileMedia.GetKind(entry.Name) is FileKind.Image or FileKind.Video or FileKind.Audio or FileKind.Text;

    private static bool CanOpenInTab(StorageEntry entry) =>
        !entry.IsDirectory && (FileMedia.GetKind(entry.Name) == FileKind.Pdf);

    private static string PreviewUrl(StorageEntry entry) => FileUrls.Preview(entry.Name, entry.LastModified);

    private static string DownloadUrl(StorageEntry entry) => FileUrls.Download(entry.Name);

    private static string? CardHref(StorageEntry entry) => CanOpenInTab(entry) ? PreviewUrl(entry) : null;

    private static string? CardTarget(StorageEntry entry) => CanOpenInTab(entry) ? "_blank" : null;

    private static string RowClass(StorageEntry entry, int index) => CanPreview(entry) ? "file-row file-row-preview" : "file-row";

    private static string KindIcon(StorageEntry entry) =>
        entry.IsDirectory
            ? Icons.Material.Filled.Folder
            : FileMedia.GetKind(entry.Name) switch
            {
                FileKind.Image => Icons.Material.Filled.Image,
                FileKind.Video => Icons.Material.Filled.Movie,
                FileKind.Audio => Icons.Material.Filled.AudioFile,
                FileKind.Text => Icons.Material.Filled.Description,
                FileKind.Pdf => Icons.Material.Filled.PictureAsPdf,
                _ => Icons.Material.Filled.InsertDriveFile
            };

    private static string FormatSize(StorageEntry entry) =>
        entry.IsDirectory
            ? "-"
            : entry.Size switch
            {
                < 1024 => $"{entry.Size} B",
                < 1024 * 1024 => $"{entry.Size / 1024d:0.#} KB",
                < 1024 * 1024 * 1024 => $"{entry.Size / 1024d / 1024d:0.#} MB",
                _ => $"{entry.Size / 1024d / 1024d / 1024d:0.#} GB"
            };

    private static string FormatLastModified(StorageEntry entry) => $"{entry.LastModified.ToLocalTime():yyyy/MM/dd HH:mm}";

    //--------------------------------------------------------------------------------
    // View
    //--------------------------------------------------------------------------------

    private enum ViewMode
    {
        List,
        Card
    }
}
