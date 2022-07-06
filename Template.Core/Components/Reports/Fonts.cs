namespace Template.Components.Reports;

using PdfSharpCore.Drawing;

public static class Fonts
{
    [ThreadStatic]
    private static XFont? largeFontB;

    public static XFont LargeFontB => largeFontB ??= new XFont(FontNames.Gothic, 28, XFontStyle.Bold);

    [ThreadStatic]
    private static XFont? normalFont;

    public static XFont NormalFont => normalFont ??= new XFont(FontNames.Gothic, 11, XFontStyle.Regular);

    [ThreadStatic]
    private static XFont? smallFont;

    public static XFont SmallFont => smallFont ??= new XFont(FontNames.Gothic, 10, XFontStyle.Regular);

    [ThreadStatic]
    private static XFont? minimumFont;

    public static XFont MinimumFont => minimumFont ??= new XFont(FontNames.Gothic, 9, XFontStyle.Regular);
}
