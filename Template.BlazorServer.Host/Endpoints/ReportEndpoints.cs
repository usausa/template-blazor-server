namespace Template.BlazorServer.Host.Endpoints;

using Template.BlazorServer.Host.Application;
using Template.BlazorServer.Host.Infrastructure.Reports;

public static class ReportEndpoints
{
    //--------------------------------------------------------------------------------
    // Mapping
    //--------------------------------------------------------------------------------

    public static void MapReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(ApiRoutes.Reports)
            .RequireAuthorization();

        group.MapGet("/invoice", HandleInvoiceAsync);
    }

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

    private static async ValueTask<IResult> HandleInvoiceAsync(
        DataService dataService,
        InvoiceReportBuilder reportBuilder,
        CancellationToken cancellationToken)
    {
        // Check data before generating (avoid empty report)
        var entities = await dataService.QueryAllAsync(cancellationToken);
        if (entities.Count == 0)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest, title: "No data.");
        }

        var bytes = reportBuilder.Build(entities);
        return TypedResults.File(bytes, "application/pdf", "invoice.pdf");
    }
}
