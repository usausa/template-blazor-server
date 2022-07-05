namespace Template.Server.Shared;

using Microsoft.AspNetCore.Components;

public interface ISectionCallback
{
    void SetMenu(RenderFragment? value);
}
