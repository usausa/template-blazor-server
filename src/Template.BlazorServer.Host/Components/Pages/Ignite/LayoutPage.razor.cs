namespace Template.BlazorServer.Host.Components.Pages.Ignite;

public sealed partial class LayoutPage
{
    private bool tileManagerVisible;

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            tileManagerVisible = true;
            StateHasChanged();
        }
    }
}
