namespace Template.BlazorServer.Host.Endpoints;

using Microsoft.Net.Http.Headers;

using Template.BlazorServer.Host.Application;
using Template.BlazorServer.Host.Reports;

public static class ReportEndpoints
{
    //--------------------------------------------------------------------------------
    // Mapping
    //--------------------------------------------------------------------------------

    public static void MapReportEndpoints(this WebApplication app)
    {
        var group = app.MapApiGroup(ApiRoutes.Reports)
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/invoice", HandleInvoiceAsync)
            .WithName("ReportInvoice")
            .Produces<Stream>(StatusCodes.Status200OK, "application/pdf")
            .ProducesProblem(StatusCodes.Status400BadRequest);
        group.MapGet("/product-list", HandleProductList)
            .WithName("ReportProductList")
            .Produces<Stream>(StatusCodes.Status200OK, "application/pdf")
            .ProducesValidationProblem();
        group.MapGet("/slip", HandleSlip)
            .WithName("ReportSlip")
            .Produces<Stream>(StatusCodes.Status200OK, "application/pdf")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

    private static async ValueTask<IResult> HandleInvoiceAsync(
        HttpContext context,
        DataService dataService,
        InvoiceReportBuilder reportBuilder,
        CancellationToken cancellationToken,
        bool download = false)
    {
        // Check data before generating (avoid empty report)
        var entities = await dataService.QueryAllAsync(cancellationToken);
        if (entities.Count == 0)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest, title: "No data.");
        }

        return Pdf(context, reportBuilder.Build(entities), "invoice.pdf", download);
    }

    private static FileContentHttpResult HandleProductList(
        HttpContext context,
        ProductListReportBuilder reportBuilder,
        [Range(1, 500)] int rows = 50,
        bool download = false) =>
        Pdf(context, reportBuilder.Build(ProductSample.LoadItems(rows)), "product-list.pdf", download);

    private static IResult HandleSlip(
        HttpContext context,
        SlipReportBuilder reportBuilder,
        string[]? sheets,
        [MaxLength(40)] string? note,
        [Range(1, 15)] int rows = 5,
        bool download = false)
    {
        var selected = new HashSet<SlipSheet>();
        foreach (var value in sheets ?? [])
        {
            if (String.IsNullOrEmpty(value) || !RequestHelper.TryParse(value, SlipSheet.Delivery, out var sheet))
            {
                return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid sheet.");
            }

            selected.Add(sheet);
        }

        if (selected.Count == 0)
        {
            selected.UnionWith(Enum.GetValues<SlipSheet>());
        }

        return Pdf(context, reportBuilder.Build(selected, ProductSample.LoadItems(rows), note), "slip.pdf", download);
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static FileContentHttpResult Pdf(HttpContext context, byte[] bytes, string fileName, bool download)
    {
        var disposition = new ContentDispositionHeaderValue(download ? "attachment" : "inline");
        disposition.SetHttpFileName(fileName);
        context.Response.Headers.ContentDisposition = disposition.ToString();
        return TypedResults.File(bytes, "application/pdf");
    }
}
