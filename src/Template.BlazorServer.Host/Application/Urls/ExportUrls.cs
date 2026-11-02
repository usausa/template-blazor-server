namespace Template.BlazorServer.Host.Application.Urls;

using System.Text.Json;

using Microsoft.AspNetCore.WebUtilities;

using Template.BlazorServer.Host.Endpoints;
using Template.BlazorServer.Host.Reports;

public static class ExportUrls
{
    public static string DataCsv => Relative(ApiRoutes.Data + "/csv");

    public static string InvoicePdf(bool download) =>
        Relative(QueryHelpers.AddQueryString(ApiRoutes.Reports + "/invoice", Query(("download", Download(download)))));

    public static string ProductListPdf(int rows, bool download) =>
        Relative(QueryHelpers.AddQueryString(ApiRoutes.Reports + "/product-list", Query(("rows", Number(rows)), ("download", Download(download)))));

    public static string SlipPdf(IEnumerable<SlipSheet> sheets, int rows, string? note, bool download) =>
        Relative(QueryHelpers.AddQueryString(
            ApiRoutes.Reports + "/slip",
            Query([.. sheets.Select(static x => ("sheets", (string?)Value(x))), ("rows", Number(rows)), ("note", String.IsNullOrEmpty(note) ? null : note), ("download", Download(download))])));

    private static string Relative(string path) => path.TrimStart('/');

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string? Download(bool download) => download ? "true" : null;

    private static string Value<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        JsonNamingPolicy.CamelCase.ConvertName(value.ToString());

    private static IEnumerable<KeyValuePair<string, string?>> Query(params (string Key, string? Value)[] items) =>
        items.Where(static x => x.Value is not null).Select(static x => KeyValuePair.Create(x.Key, x.Value));
}
