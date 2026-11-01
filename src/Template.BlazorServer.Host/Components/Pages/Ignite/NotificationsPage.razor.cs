namespace Template.BlazorServer.Host.Components.Pages.Ignite;

using IgniteUI.Blazor.Controls;

public sealed partial class NotificationsPage
{
#pragma warning disable CA2213
    private IgbBanner banner = default!;

    private IgbDialog dialog = default!;

    private IgbSnackbar snackbar = default!;

    private IgbToast toast = default!;
#pragma warning restore CA2213

    private string? dialogResult;

    private int undoCount;

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private Task<bool> OnBannerShowAsync() => banner.ShowAsync();

    private Task<bool> OnBannerHideAsync() => banner.HideAsync();

    private Task<bool> OnDialogShowAsync() => dialog.ShowAsync();

    private Task<bool> OnDialogCancelAsync()
    {
        dialogResult = "Cancel";
        return dialog.HideAsync();
    }

    private Task<bool> OnDialogSaveAsync()
    {
        dialogResult = "Save";
        return dialog.HideAsync();
    }

    private Task<bool> OnSnackbarShowAsync() => snackbar.ShowAsync();

    private void OnSnackbarAction(IgbVoidEventArgs args) => undoCount++;

    private Task<bool> OnToastShowAsync() => toast.ShowAsync();
}
