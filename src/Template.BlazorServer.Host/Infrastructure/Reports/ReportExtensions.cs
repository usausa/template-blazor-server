namespace Template.BlazorServer.Host.Infrastructure.Reports;

using OysterReport;

public static class ReportExtensions
{
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
