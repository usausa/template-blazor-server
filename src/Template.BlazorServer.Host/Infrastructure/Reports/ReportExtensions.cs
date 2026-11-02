namespace Template.BlazorServer.Host.Infrastructure.Reports;

using OysterReport;

using QRCoder;

public static class ReportExtensions
{
    private const int QrCodePixelsPerModule = 10;

    public static void FillRows(this TemplateSheet sheet, string marker, IReadOnlyList<IReadOnlyDictionary<string, string?>> rows)
    {
        var row = sheet.FindRow(marker);
        if (rows.Count == 0)
        {
            row.Delete();
            return;
        }

        for (var i = 1; i < rows.Count; i++)
        {
            row = row.InsertCopyBelow();
        }

        sheet.ReplacePlaceholders(rows);
    }

    public static void ReplaceOrDeleteRow(this TemplateSheet sheet, string marker, string? value)
    {
        if (String.IsNullOrEmpty(value))
        {
            sheet.FindRow(marker).Delete();
        }
        else
        {
            sheet.ReplacePlaceholder(marker, value);
        }
    }

    public static int ReplacePlaceholderWithQrCode(this TemplateWorkbook workbook, string marker, string text)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
        using var code = new PngByteQRCode(data);
        return workbook.ReplacePlaceholderWithImage(marker, code.GetGraphic(QrCodePixelsPerModule));
    }

    public static IReadOnlyList<TemplateSheet> CopyPages(this TemplateWorkbook workbook, string sheetName, int pageCount)
    {
        var pages = new List<TemplateSheet>(pageCount);
        for (var i = 1; i <= pageCount; i++)
        {
            pages.Add(workbook.CopySheet(sheetName, $"{sheetName} ({i})"));
        }

        workbook.RemoveSheet(sheetName);
        return pages;
    }
}
