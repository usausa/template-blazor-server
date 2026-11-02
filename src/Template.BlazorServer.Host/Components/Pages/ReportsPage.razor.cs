namespace Template.BlazorServer.Host.Components.Pages;

using Template.BlazorServer.Host.Reports;

public sealed partial class ReportsPage
{
    private static readonly int[] ProductRowOptions = [10, 50, 100, 200];

    private int productRows = 50;

    private bool delivery = true;

    private bool invoice = true;

    private bool receipt = true;

    private int slipRows = 5;

    private string? note;

    private bool HasSlipSheet => delivery || invoice || receipt;

    private IEnumerable<SlipSheet> SlipSheets
    {
        get
        {
            if (delivery)
            {
                yield return SlipSheet.Delivery;
            }

            if (invoice)
            {
                yield return SlipSheet.Invoice;
            }

            if (receipt)
            {
                yield return SlipSheet.Receipt;
            }
        }
    }
}
