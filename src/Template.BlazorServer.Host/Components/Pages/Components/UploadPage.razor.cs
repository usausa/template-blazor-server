namespace Template.BlazorServer.Host.Components.Pages.Components;

using BlazorComponents;

public sealed partial class UploadPage
{
#pragma warning disable CA2213
    private FileUpload upload = default!;
#pragma warning restore CA2213

    private bool uploading;

    private FileUploadProgress? progress;

    private IReadOnlyList<FileUploadError> errors = [];

    private string result = "-";

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    private int ProgressPercent => (progress is null) || (progress.TotalBytes == 0) ? 0 : (int)(progress.LoadedBytes * 100 / progress.TotalBytes);

    private string ProgressText =>
        progress is null
            ? string.Empty
            : $"{progress.CompletedFiles} / {progress.TotalFiles} files, {FormatSize(progress.LoadedBytes)} / {FormatSize(progress.TotalBytes)} {progress.CurrentPath}";

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnStarting(FileUploadStartingEventArgs args)
    {
        progress = null;
        errors = [];
        result = $"Starting: {args.FileCount} files, {FormatSize(args.TotalBytes)}";
    }

    private void OnProgress(FileUploadProgress value) => progress = value;

    private void OnCompleted(FileUploadResult value)
    {
        errors = value.Errors;
        result = value.Canceled
            ? $"Canceled: {value.Succeeded} succeeded, {value.Failed} failed"
            : $"Completed: {value.Succeeded} succeeded, {value.Failed} failed";
    }

    private Task SelectFilesAsync() => upload.OpenFilePickerAsync().AsTask();

    private Task SelectFolderAsync() => upload.OpenFolderPickerAsync().AsTask();

    private Task CancelAsync() => upload.CancelAsync().AsTask();

    //--------------------------------------------------------------------------------
    // Format
    //--------------------------------------------------------------------------------

    private static string FormatSize(long size) =>
        size switch
        {
            < 1024 => $"{size} B",
            < 1024 * 1024 => $"{size / 1024d:0.#} KB",
            < 1024 * 1024 * 1024 => $"{size / 1024d / 1024d:0.#} MB",
            _ => $"{size / 1024d / 1024d / 1024d:0.#} GB"
        };
}
