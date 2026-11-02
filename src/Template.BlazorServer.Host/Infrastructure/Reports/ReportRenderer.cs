namespace Template.BlazorServer.Host.Infrastructure.Reports;

using OysterReport;

public sealed class ReportRenderer
{
    private const string FontDirectory = "Assets/Fonts";
    private const string FontName = "IPAexGothic";

    private static readonly string[] AliasFontNames =
    [
        "ＭＳ Ｐゴシック",
        "MS Pゴシック",
        "ＭＳ ゴシック",
        "MS ゴシック",
        "メイリオ",
        "Meiryo",
        "游ゴシック",
        "Yu Gothic",
        "游ゴシック Medium",
        "Yu Gothic Medium",
        "ＭＳ Ｐ明朝",
        "MS P明朝",
        "ＭＳ 明朝",
        "MS 明朝",
        "HGP明朝E",
        "HGPMinchoE",
        "HGS明朝E",
        "HGSMinchoE"
    ];

    private readonly Lock sync = new();

    private readonly Dictionary<string, TemplateWorkbook> templates = new(StringComparer.Ordinal);

    private readonly ReportRenderOption option;

    private readonly OysterReportEngine engine;

    public ReportRenderer(ILogger<ReportRenderer> log)
    {
        option = new ReportRenderOption
        {
            UseSystemFonts = false,
            FontDirectories = [FontDirectory],
            FontAliases = AliasFontNames.ToDictionary(static x => x, static IReadOnlyList<string> (_) => [FontName], StringComparer.OrdinalIgnoreCase),
            HeaderFooterFallbackFonts = [FontName],
            OnRenderWarning = warning => log.WarnRenderWarning(warning.Kind, warning.SheetName, warning.Source, warning.Message)
        };
        engine = new OysterReportEngine
        {
            RenderingOptions = option
        };
    }

    public TemplateWorkbook LoadTemplate(string path)
    {
        lock (sync)
        {
            if (!templates.TryGetValue(path, out var template))
            {
                template = new TemplateWorkbook(path, option);
                templates[path] = template;
            }

            return template.Clone();
        }
    }

    public byte[] Render(TemplateWorkbook workbook)
    {
        using var output = new MemoryStream();
        engine.GeneratePdf(workbook, output);
        return output.ToArray();
    }
}
