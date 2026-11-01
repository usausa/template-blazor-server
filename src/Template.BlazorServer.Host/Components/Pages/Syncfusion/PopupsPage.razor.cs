namespace Template.BlazorServer.Host.Components.Pages.Syncfusion;

public sealed partial class PopupsPage
{
    private bool dialogVisible;

    private string? dialogResult;

    private bool loading;

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnDialogOpen() => dialogVisible = true;

    private void OnDialogCancel()
    {
        dialogResult = "Cancel";
        dialogVisible = false;
    }

    private void OnDialogSave()
    {
        dialogResult = "Save";
        dialogVisible = false;
    }

    private async Task OnLoadAsync()
    {
        loading = true;
        await Task.Delay(2000);
        loading = false;
    }
}
