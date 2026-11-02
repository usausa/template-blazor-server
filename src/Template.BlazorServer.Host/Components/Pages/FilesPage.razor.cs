namespace Template.BlazorServer.Host.Components.Pages;

using BlazorComponents;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using MudBlazor;

using Template.BlazorServer.Host.Application.Urls;
using Template.BlazorServer.Host.Components.Dialogs;
using Template.BlazorServer.Host.Components.Shared;
using Template.BlazorServer.Host.Infrastructure.IO;
using Template.BlazorServer.Infrastructure.Storage;

public sealed partial class FilesPage
{
    private const long MaxFileSize = 100L * 1024 * 1024;

    private const int MaxQueueCount = 20;

    private const int ThumbnailSize = 96;

    private const long MaxThumbnailSize = 1024 * 1024;

    private const int ResizeSize = 1920;

    private readonly List<InputSlot> slots = [new()];

    private readonly List<UploadItem> queue = [];

#pragma warning disable CA2213
    private FileDropZone dropZone = default!;
#pragma warning restore CA2213

    private StorageEntry[] entries = [];

    private ViewMode view = ViewMode.List;

    private CancellationTokenSource? cancellation;

    private bool uploading;

    private bool resize;

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

    private bool HasQueue => queue.Count > 0;

    private string QueueSummary =>
        uploading
            ? $"送信中 {Math.Min(queue.Count(static x => x.State == UploadState.Done) + 1, queue.Count)} / {queue.Count} 件"
            : $"{queue.Count} 件・{FormatSize(queue.Sum(static x => x.File.Size))}";

    private int TotalProgress
    {
        get
        {
            var total = queue.Sum(static x => x.File.Size);
            var sent = queue.Sum(static x => x.State == UploadState.Done ? x.File.Size : x.File.Size * x.Progress / 100);
            return total == 0 ? 0 : (int)(sent * 100 / total);
        }
    }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override Task OnInitializedAsync() =>
        LoadAsync();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            cancellation?.Cancel();
            cancellation?.Dispose();
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private Task OnFilesChangedAsync(InputSlot slot, InputFileChangeEventArgs args)
    {
        var accepted = new List<UploadItem>();
        var remaining = MaxQueueCount - queue.Count;
        foreach (var file in args.GetMultipleFiles(args.FileCount))
        {
            if (accepted.Count >= remaining)
            {
                Snackbar.AddWarning($"一度に送れるのは {MaxQueueCount} 件までです。");
                break;
            }

            if (file.Size > MaxFileSize)
            {
                Snackbar.AddWarning($"{file.Name} は 100 MB を超えています。");
                continue;
            }

            accepted.Add(new UploadItem(slot, file, MakeUniqueName(Path.GetFileName(file.Name), accepted)));
        }

        queue.AddRange(accepted);
        slots.Insert(0, new InputSlot());
        ReleaseSlots();
        StateHasChanged();

        return Task.WhenAll(accepted.Where(static x => FileMedia.GetKind(x.Name) == FileKind.Image).Select(LoadThumbnailAsync));
    }

    private Task OnRowClickAsync(DataGridRowClickEventArgs<StorageEntry> args) =>
        CanPreview(args.Item) ? PreviewAsync(args.Item) : Task.CompletedTask;

    private void OnProgress(UploadItem item, int percent)
    {
        _ = InvokeAsync(() =>
        {
            item.Progress = percent;
            StateHasChanged();
        });
    }

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

    private Task SelectAsync() =>
        dropZone.OpenFilePickerAsync().AsTask();

    private Task OpenAsync(StorageEntry entry) =>
        CanPreview(entry) ? PreviewAsync(entry) : Task.CompletedTask;

    private async Task PreviewAsync(StorageEntry entry)
    {
        await DialogService.ShowFilePreviewDialog(entry);
    }

    private void Remove(UploadItem item)
    {
        queue.Remove(item);
        ReleaseSlots();
    }

    private void Clear()
    {
        queue.Clear();
        ReleaseSlots();
    }

    private void Cancel() => cancellation?.Cancel();

    private async Task UploadAsync()
    {
        var existing = entries.Where(static x => !x.IsDirectory).Select(static x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var duplicates = queue.Where(x => existing.Contains(x.Name)).Select(static x => x.Name).ToList();
        if ((duplicates.Count > 0) &&
            !await DialogService.ShowConfirm("上書きの確認", $"同じ名前のファイルがあります: {String.Join("、", duplicates)}。上書きしてよろしいですか？"))
        {
            return;
        }

        foreach (var item in queue)
        {
            item.State = UploadState.Waiting;
            item.Progress = 0;
        }

        uploading = true;
        cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var canceled = false;
        try
        {
            foreach (var item in queue.ToArray())
            {
                item.State = UploadState.Sending;
                StateHasChanged();
                try
                {
                    var file = resize && CanResize(item.File) ? await item.File.RequestImageFileAsync(item.File.ContentType, ResizeSize, ResizeSize) : item.File;
                    await using var browser = file.OpenReadStream(MaxFileSize, token);
                    await using var progressStream = new ReadProgressStream(browser, file.Size, percent => OnProgress(item, percent));
                    await Storage.WriteAsync(item.Name, progressStream, token);
                    item.State = UploadState.Done;
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    item.State = UploadState.Waiting;
                    item.Progress = 0;
                    canceled = true;
                    break;
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException or StorageException or JSException or OperationCanceledException)
                {
                    item.State = UploadState.Failed;
                }
            }
        }
        finally
        {
            uploading = false;
            cancellation.Dispose();
            cancellation = null;
        }

        var uploaded = queue.RemoveAll(static x => x.State == UploadState.Done);
        ReleaseSlots();
        if (uploaded > 0)
        {
            Snackbar.AddSuccess($"{uploaded} 件をアップロードしました。");
        }

        if (canceled)
        {
            Snackbar.AddInfo("アップロードを取り消しました。");
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
    // Helper
    //--------------------------------------------------------------------------------

    private string MakeUniqueName(string name, IEnumerable<UploadItem> accepted)
    {
        var names = queue.Concat(accepted).Select(static x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!names.Contains(name))
        {
            return name;
        }

        var stem = Path.GetFileNameWithoutExtension(name);
        var extension = Path.GetExtension(name);
        var number = 2;
        while (names.Contains($"{stem} ({number}){extension}"))
        {
            number++;
        }

        return $"{stem} ({number}){extension}";
    }

    private void ReleaseSlots()
    {
        var fresh = slots[0];
        slots.RemoveAll(x => (x != fresh) && queue.All(y => y.Slot != x));
    }

    private async Task LoadThumbnailAsync(UploadItem item)
    {
        try
        {
            var thumbnail = await item.File.RequestImageFileAsync("image/png", ThumbnailSize, ThumbnailSize);
            await using var stream = thumbnail.OpenReadStream(MaxThumbnailSize);
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            item.Thumbnail = $"data:image/png;base64,{Convert.ToBase64String(buffer.ToArray())}";
            StateHasChanged();
        }
        catch (Exception e) when (e is IOException or JSException or OperationCanceledException)
        {
        }
    }

    //--------------------------------------------------------------------------------
    // Format
    //--------------------------------------------------------------------------------

    private static bool IsSending(UploadItem item) => item.State == UploadState.Sending;

    private static bool IsDone(UploadItem item) => item.State == UploadState.Done;

    private static bool IsFailed(UploadItem item) => item.State == UploadState.Failed;

    private static bool CanResize(IBrowserFile file) =>
        file.ContentType is "image/jpeg" or "image/png";

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
        entry.IsDirectory ? Icons.Material.Filled.Folder : KindIcon(entry.Name);

    private static string KindIcon(string name) =>
        FileMedia.GetKind(name) switch
        {
            FileKind.Image => Icons.Material.Filled.Image,
            FileKind.Video => Icons.Material.Filled.Movie,
            FileKind.Audio => Icons.Material.Filled.AudioFile,
            FileKind.Text => Icons.Material.Filled.Description,
            FileKind.Pdf => Icons.Material.Filled.PictureAsPdf,
            _ => Icons.Material.Filled.InsertDriveFile
        };

    private static string FormatSize(StorageEntry entry) =>
        entry.IsDirectory ? "-" : FormatSize(entry.Size);

    private static string FormatSize(long size) =>
        size switch
        {
            < 1024 => $"{size} B",
            < 1024 * 1024 => $"{size / 1024d:0.#} KB",
            < 1024 * 1024 * 1024 => $"{size / 1024d / 1024d:0.#} MB",
            _ => $"{size / 1024d / 1024d / 1024d:0.#} GB"
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

    private enum UploadState
    {
        Waiting,
        Sending,
        Done,
        Failed
    }

    private sealed class InputSlot;

    private sealed class UploadItem
    {
        public InputSlot Slot { get; }

        public IBrowserFile File { get; }

        public string Name { get; }

        public string? Thumbnail { get; set; }

        public UploadState State { get; set; }

        public int Progress { get; set; }

        public UploadItem(InputSlot slot, IBrowserFile file, string name)
        {
            Slot = slot;
            File = file;
            Name = name;
        }
    }
}
