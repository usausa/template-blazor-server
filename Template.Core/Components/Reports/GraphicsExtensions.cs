namespace Template.Components.Reports;

using PdfSharpCore.Drawing;

public static class GraphicsExtensions
{
    [ThreadStatic]
    private static XPen? pen;

    private static XPen Pen => pen ??= new XPen(XColors.Black, 1);

    public static void DrawStringCenter(this XGraphics g, string s, XFont font, double x, double y)
    {
        g.DrawString(s, font, XBrushes.Black, x, y, XStringFormats.Center);
    }

    public static void DrawStringCenterLeft(this XGraphics g, string s, XFont font, double x, double y)
    {
        g.DrawString(s, font, XBrushes.Black, x, y, XStringFormats.CenterLeft);
    }

    public static void DrawStringTopLeft(this XGraphics g, string s, XFont font, double x, double y)
    {
        g.DrawString(s, font, XBrushes.Black, x, y, XStringFormats.TopLeft);
    }

    public static void DrawStringTopRight(this XGraphics g, string s, XFont font, double x, double y)
    {
        g.DrawString(s, font, XBrushes.Black, x, y, XStringFormats.TopRight);
    }

    public static void DrawStringTopCenter(this XGraphics g, string s, XFont font, double x, double y)
    {
        g.DrawString(s, font, XBrushes.Black, x, y, XStringFormats.TopCenter);
    }

    public static void DrawStringBottomRight(this XGraphics g, string s, XFont font, double x, double y)
    {
        g.DrawString(s, font, XBrushes.Black, x, y, XStringFormats.BottomRight);
    }

    public static void DrawStringBottomLeft(this XGraphics g, string s, XFont font, double x, double y)
    {
        g.DrawString(s, font, XBrushes.Black, x, y, XStringFormats.BottomLeft);
    }

    public static void DrawStringCenter(this XGraphics g, string s, XFont font, double x, double y, double width, double height)
    {
        g.DrawString(s, font, XBrushes.Black, new XRect(x, y, width, height), XStringFormats.Center);
    }

    public static void DrawStringCenterRight(this XGraphics g, string s, XFont font, double x, double y, double width, double height, double padding)
    {
        g.DrawString(s, font, XBrushes.Black, new XRect(x, y, width - padding, height), XStringFormats.CenterRight);
    }

    public static void DrawStringCenterLeft(this XGraphics g, string s, XFont font, double x, double y, double width, double height, double padding)
    {
        g.DrawString(s, font, XBrushes.Black, new XRect(x + padding, y, width - padding, height), XStringFormats.CenterLeft);
    }

    public static void DrawStringBottomCenter(this XGraphics g, string s, XFont font, double x, double y, double width, double height, double padding)
    {
        g.DrawString(s, font, XBrushes.Black, new XRect(x + padding, y, width - padding, height), XStringFormats.BottomCenter);
    }

    public static void DrawLine(this XGraphics g, double x1, double y1, double x2, double y2)
    {
        g.DrawLine(Pen, x1, y1, x2, y2);
    }

    public static void DrawRectangle(this XGraphics g, double x, double y, double width, double height)
    {
        g.DrawRectangle(Pen, x, y, width, height);
    }
}
