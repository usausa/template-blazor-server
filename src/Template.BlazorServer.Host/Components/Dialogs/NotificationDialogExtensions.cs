namespace Template.BlazorServer.Host.Components.Dialogs;

using MudBlazor;

public static class NotificationDialogExtensions
{
    public static async ValueTask<NotificationSendRequest?> ShowNotificationSendDialog(this IDialogService dialog, IReadOnlyList<string> names)
    {
        var reference = await dialog.ShowAsync<NotificationSendDialog>(
            string.Empty,
            new DialogParameters
            {
                { nameof(NotificationSendDialog.Names), names }
            });
        var result = await reference.Result;
        return (result is { Canceled: false }) ? (NotificationSendRequest)result.Data! : null;
    }
}
