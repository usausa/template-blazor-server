namespace Template.BlazorServer.Host.Components.Dialogs;

public sealed record NotificationSendRequest(string? UserName, string Title, string Body, string? Link);
