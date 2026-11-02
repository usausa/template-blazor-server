namespace Template.BlazorServer.Infrastructure.Reports;

using OysterReport;

using Template.BlazorServer.Host.Infrastructure.Reports;

public sealed class ReportExtensionsTests
{
    private static readonly string ProductListPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Reports", "ProductList.xlsx");

    private static readonly string SlipPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Reports", "Slip.xlsx");

    [Fact]
    public void FillRowsCopiesMarkerRowForEachRow()
    {
        // Arrange
        using var workbook = new TemplateWorkbook(ProductListPath);
        var sheet = workbook.GetSheet("商品一覧");
        var row = sheet.FindRow("No").RowNumber;

        // Act
        sheet.FillRows("No", [MakeRow("1"), MakeRow("2"), MakeRow("3")]);

        // Assert
        Assert.Equal("1", sheet.GetCellText(row, 1));
        Assert.Equal("2", sheet.GetCellText(row + 1, 1));
        Assert.Equal("3", sheet.GetCellText(row + 2, 1));
        Assert.Equal("合計", sheet.GetCellText(row + 3, 1));
    }

    [Fact]
    public void FillRowsDeletesMarkerRowWhenNoRow()
    {
        // Arrange
        using var workbook = new TemplateWorkbook(ProductListPath);
        var sheet = workbook.GetSheet("商品一覧");
        var row = sheet.FindRow("No").RowNumber;

        // Act
        sheet.FillRows("No", []);

        // Assert
        Assert.False(sheet.TryFindMarkerPosition("No", out _));
        Assert.Equal("合計", sheet.GetCellText(row, 1));
    }

    [Fact]
    public void ReplaceOrDeleteRowReplacesValue()
    {
        // Arrange
        using var workbook = new TemplateWorkbook(SlipPath);
        var sheet = workbook.GetSheet("納品書");
        var (row, column) = sheet.FindMarkerPosition("Note");

        // Act
        sheet.ReplaceOrDeleteRow("Note", "午前中に納品");

        // Assert
        Assert.Equal("午前中に納品", sheet.GetCellText(row, column));
    }

    [Fact]
    public void ReplaceOrDeleteRowDeletesRowWhenEmpty()
    {
        // Arrange
        using var workbook = new TemplateWorkbook(SlipPath);
        var sheet = workbook.GetSheet("納品書");
        var (row, _) = sheet.FindMarkerPosition("Note");

        // Act
        sheet.ReplaceOrDeleteRow("Note", string.Empty);

        // Assert
        Assert.False(sheet.TryFindMarkerPosition("Note", out _));
        Assert.NotEqual("備考", sheet.GetCellText(row, 1));
    }

    [Fact]
    public void CopyPagesReplacesTemplateSheetWithPages()
    {
        // Arrange
        using var workbook = new TemplateWorkbook(ProductListPath);
        string[] expected = ["商品一覧 (1)", "商品一覧 (2)", "商品一覧 (3)"];

        // Act
        var pages = workbook.CopyPages("商品一覧", 3);

        // Assert
        Assert.Equal(expected, pages.Select(static x => x.Name));
        Assert.Equal(expected, workbook.Sheets.Select(static x => x.Name));
    }

    private static Dictionary<string, string?> MakeRow(string no) => new() { ["No"] = no };
}
