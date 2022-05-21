namespace Template.Server.Infrastructure;

using MudBlazor;

public static class Styles
{
    public static MudTheme Theme => new()
    {
        Palette = new Palette
        {
            Primary = Colors.Blue.Default,
            Secondary = Colors.Green.Accent4,
            AppbarBackground = Colors.Blue.Darken4
        }
    };
}
