namespace Template.BlazorServer.Host.Components.Pages.Syncfusion;

public sealed partial class ButtonsPage
{
    private static readonly string[] Alignments = ["Left", "Center", "Right"];

    private int clickCount;

    private string alignment = "Left";

    private bool agreed;

    private string plan = "basic";

    private bool notifications = true;

    private bool wifi;

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnButtonClick() => clickCount++;

    private void OnAlignmentChanged(bool selected, string value)
    {
        if (selected)
        {
            alignment = value;
        }
    }
}
