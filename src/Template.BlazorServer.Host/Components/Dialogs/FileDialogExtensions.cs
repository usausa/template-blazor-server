namespace Template.BlazorServer.Host.Components.Dialogs;

using MudBlazor;

using Template.BlazorServer.Infrastructure.Storage;

public static class FileDialogExtensions
{
    public static async ValueTask ShowFilePreviewDialog(this IDialogService dialog, StorageEntry entry)
    {
        var reference = await dialog.ShowAsync<FilePreviewDialog>(
            string.Empty,
            new DialogParameters
            {
                { nameof(FilePreviewDialog.Entry), entry }
            },
            new DialogOptions
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                CloseButton = true
            });
        await reference.Result;
    }
}
