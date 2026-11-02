namespace Template.BlazorServer.Host.Reports;

using Template.BlazorServer.Host.Infrastructure.Reports;

public sealed class InvoiceReportBuilder
{
    private const string TemplatePath = "Assets/Reports/Invoice.xlsx";
    private const string SheetName = "請求書";
    private const int RowsPerPage = 25;

    private readonly TimeProvider timeProvider;

    private readonly ReportRenderer renderer;

    public InvoiceReportBuilder(TimeProvider timeProvider, ReportRenderer renderer)
    {
        this.timeProvider = timeProvider;
        this.renderer = renderer;
    }

    public byte[] Build(IReadOnlyList<DataEntity> entities)
    {
        var today = timeProvider.GetLocalNow();
        var subTotal = entities.Sum(static e => (long)e.Value);
        var tax = (long)(subTotal * 0.1);
        var chunks = entities.Chunk(RowsPerPage).ToList();

        using var workbook = renderer.LoadTemplate(TemplatePath);
        var pages = workbook.CopyPages(SheetName, chunks.Count);
        foreach (var sheet in pages.SkipLast(1))
        {
            sheet.FindRows("SubTotal").Delete();
        }

        workbook.ReplacePlaceholders(new Dictionary<string, string?>
        {
            ["Subject"] = "御請求書",
            ["BillingTo"] = "サンプル株式会社",
            ["InvoiceDate"] = ReportText.Date(today),
            ["InvoiceNo"] = $"INV-{today:yyyyMMdd}-001",
            ["DeliveryDate"] = ReportText.Date(today),
            ["SubTotal"] = ReportText.Number(subTotal),
            ["Tax"] = ReportText.Number(tax),
            ["TotalAmount"] = ReportText.Number(subTotal + tax),
            ["PageCount"] = ReportText.Number(chunks.Count)
        });

        for (var i = 0; i < chunks.Count; i++)
        {
            var offset = i * RowsPerPage;
            pages[i].ReplacePlaceholder("Page", ReportText.Number(i + 1));
            pages[i].ReplacePlaceholders(chunks[i].Select((e, j) => new Dictionary<string, string?>
            {
                ["No"] = ReportText.Number(offset + j + 1),
                ["Item"] = e.Name,
                ["Qty"] = "1",
                ["Price"] = ReportText.Number(e.Value),
                ["Amount"] = ReportText.Number(e.Value)
            }));
        }

        return renderer.Render(workbook);
    }
}
