namespace Template.Server.Shared;

using Microsoft.AspNetCore.Components;

public interface IMenuSectionCallback
{
    void SetMenu(RenderFragment? value);
}
