namespace Template.BlazorServer.Host.Components.Pages.Ignite;

public sealed partial class DisplayPage
{
    private static readonly string[] IconNames = ["home", "search", "mail", "phone", "folder", "star", "favorite", "settings"];

    private double progress = 65;

    private string? qrText = "https://github.com/IgniteUI/igniteui-blazor";

    private string? searchText = "components";
}
