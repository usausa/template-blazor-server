namespace Template.Server.Shared;

public sealed partial class SimpleLayout
{
    private ErrorBoundary? errorBoundary;

    protected override void OnParametersSet()
    {
        errorBoundary?.Recover();
    }
}
