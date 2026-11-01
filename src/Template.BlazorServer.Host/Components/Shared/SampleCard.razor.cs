namespace Template.BlazorServer.Host.Components.Shared;

using Microsoft.AspNetCore.Components;

public sealed partial class SampleCard
{
    //--------------------------------------------------------------------------------
    // Parameter
    //--------------------------------------------------------------------------------

    [Parameter]
    public required string Title { get; set; }

    [Parameter]
    public string? Result { get; set; }

    [Parameter]
    public bool Wide { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string CssClass => Wide ? "sample-card sample-wide" : "sample-card";
}
