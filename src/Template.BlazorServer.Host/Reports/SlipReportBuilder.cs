namespace Template.BlazorServer.Host.Reports;

using Template.BlazorServer.Host.Infrastructure.Reports;

public sealed class SlipReportBuilder
{
    private const string TemplatePath = "Assets/Reports/Slip.xlsx";

    private static readonly Dictionary<SlipSheet, string> SheetNames = new()
    {
        [SlipSheet.Delivery] = "納品書",
        [SlipSheet.Invoice] = "請求書",
        [SlipSheet.Receipt] = "受領書"
    };

    private readonly TimeProvider timeProvider;

    private readonly ReportRenderer renderer;

    public SlipReportBuilder(TimeProvider timeProvider, ReportRenderer renderer)
    {
        this.timeProvider = timeProvider;
        this.renderer = renderer;
    }

    public byte[] Build(IReadOnlyCollection<SlipSheet> sheets, IReadOnlyList<ProductItem> items, string? note)
    {
        var today = timeProvider.GetLocalNow();
        var slipNo = $"{today:yyyyMMdd}-0001";
        var subTotal = items.Sum(static x => x.Amount);
        var tax = (long)(subTotal * 0.1);

        using var workbook = renderer.LoadTemplate(TemplatePath);
        foreach (var (sheet, name) in SheetNames)
        {
            if (!sheets.Contains(sheet))
            {
                workbook.RemoveSheet(name);
            }
        }

        workbook.ReplacePlaceholders(new Dictionary<string, string?>
        {
            ["CustomerName"] = "サンプル株式会社",
            ["SlipNo"] = slipNo,
            ["IssueDate"] = ReportText.Date(today),
            ["SubTotal"] = ReportText.Number(subTotal),
            ["Tax"] = ReportText.Number(tax),
            ["Total"] = ReportText.Number(subTotal + tax)
        });
        workbook.ReplacePlaceholderWithQrCode("QR", slipNo);

        var rows = items.Select((x, i) => new Dictionary<string, string?>
        {
            ["No"] = ReportText.Number(i + 1),
            ["Name"] = x.Name,
            ["Quantity"] = ReportText.Number(x.Quantity),
            ["Price"] = ReportText.Number(x.Price),
            ["Amount"] = ReportText.Number(x.Amount)
        }).ToList();
        foreach (var sheet in workbook.Sheets)
        {
            sheet.FillRows("No", rows);
            sheet.ReplaceOrDeleteRow("Note", note);
        }

        return renderer.Render(workbook);
    }
}
