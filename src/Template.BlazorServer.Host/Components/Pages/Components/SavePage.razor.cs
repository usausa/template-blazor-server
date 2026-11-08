namespace Template.BlazorServer.Host.Components.Pages.Components;

using System.Text;

using BlazorComponents;

using Microsoft.AspNetCore.Components;

using Template.BlazorServer.Host.Application.Urls;

public sealed partial class SavePage
{
    private const int MaxRows = 100000;

    private string text = "The quick brown fox jumps over the lazy dog.";

    private int rows = 1000;

    private string? textResult;

    private string? csvResult;

    private string? urlResult;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required IFileSystemAccess FileSystemAccess { get; set; }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private async Task SaveTextAsync()
    {
        var result = await FileSystemAccess.SaveAsync("sample.txt", "text/plain", Encoding.UTF8.GetBytes(text));
        textResult = result.ToString();
    }

    private async Task SaveCsvAsync()
    {
        await using var stream = CreateCsv(rows);
        var result = await FileSystemAccess.SaveAsync("sample.csv", "text/csv", stream);
        csvResult = result.ToString();
    }

    private async Task SaveUrlAsync()
    {
        var result = await FileSystemAccess.SaveAsync("data.csv", "text/csv", new Uri(ExportUrls.DataCsv, UriKind.Relative));
        urlResult = result.ToString();
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static MemoryStream CreateCsv(int count)
    {
        var builder = new StringBuilder("No,Name,Value\r\n");
        for (var i = 1; i <= count; i++)
        {
            builder.Append(CultureInfo.InvariantCulture, $"{i},Item {i},{i * 10}\r\n");
        }

        return new MemoryStream(Encoding.UTF8.GetBytes(builder.ToString()));
    }
}
