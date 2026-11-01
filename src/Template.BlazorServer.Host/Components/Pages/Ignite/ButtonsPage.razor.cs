namespace Template.BlazorServer.Host.Components.Pages.Ignite;

public sealed partial class ButtonsPage
{
    private int clickCount;

    private bool favorite;

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnButtonClick() => clickCount++;

    private void OnFavoriteClick() => favorite = !favorite;
}
