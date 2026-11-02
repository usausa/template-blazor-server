namespace Template.BlazorServer.Host.Reports;

using Template.BlazorServer.Host.Infrastructure.Reports;

public sealed class ProductListReportBuilder
{
    private const string TemplatePath = "Assets/Reports/ProductList.xlsx";
    private const string SheetName = "商品一覧";
    private const int RowsPerPage = 20;

    private readonly TimeProvider timeProvider;

    private readonly ReportRenderer renderer;

    public ProductListReportBuilder(TimeProvider timeProvider, ReportRenderer renderer)
    {
        this.timeProvider = timeProvider;
        this.renderer = renderer;
    }

    public byte[] Build(IReadOnlyList<ProductItem> items)
    {
        var chunks = items.Chunk(RowsPerPage).ToList();

        using var workbook = renderer.LoadTemplate(TemplatePath);
        var pages = workbook.CopyPages(SheetName, chunks.Count);
        for (var i = 0; i < chunks.Count; i++)
        {
            var offset = i * RowsPerPage;
            pages[i].FillRows("No", chunks[i].Select((x, j) => new Dictionary<string, string?>
            {
                ["No"] = ReportText.Number(offset + j + 1),
                ["Code"] = x.Code,
                ["Name"] = x.Name,
                ["Category"] = x.Category,
                ["Quantity"] = ReportText.Number(x.Quantity),
                ["Price"] = ReportText.Number(x.Price),
                ["Amount"] = ReportText.Number(x.Amount)
            }).ToList());
        }

        foreach (var sheet in pages.SkipLast(1))
        {
            sheet.FindRow("TotalAmount").Delete();
        }

        workbook.ReplacePlaceholders(new Dictionary<string, string?>
        {
            ["PrintedAt"] = ReportText.DateTime(timeProvider.GetLocalNow()),
            ["ItemCount"] = ReportText.Number(items.Count),
            ["TotalQuantity"] = ReportText.Number(items.Sum(static x => (long)x.Quantity)),
            ["TotalAmount"] = ReportText.Number(items.Sum(static x => x.Amount))
        });

        return renderer.Render(workbook);
    }
}
