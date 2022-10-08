namespace Template.Server.Shared;

public sealed partial class ErrorDispatcher
{
    [Parameter]
    public Exception? Exception { get; set; }

    [Parameter]
    public EventCallback RecoverRequest { get; set; }
}
