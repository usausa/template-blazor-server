namespace Template.BlazorServer.Host.Components.Pages.Components;

public sealed partial class DisplayPage
{
    private bool narrow;

    private bool dark;

    private bool reducedMotion;

    private bool modalOpen;

    private bool closeOnEscape = true;

    private bool closeOnBackdropClick;

    private string modalResult = "-";

    private bool modelessOpen;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    private string MediaClass => narrow ? "sample-media sample-media-narrow" : "sample-media";

    private string WidthText => narrow ? "narrow" : "wide";

    private string SchemeText => dark ? "dark" : "light";

    private string MotionText => reducedMotion ? "reduce" : "no preference";

    private string MediaResult => $"Narrow: {narrow} / Dark: {dark} / Reduced motion: {reducedMotion}";

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OpenModal() => modalOpen = true;

    private void OnModalClose(string value) => modalResult = String.IsNullOrEmpty(value) ? "(none)" : value;

    private void ToggleModeless() => modelessOpen = !modelessOpen;
}
