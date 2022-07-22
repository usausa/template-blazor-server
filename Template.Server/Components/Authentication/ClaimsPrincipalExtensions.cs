namespace Template.Server.Components.Authentication;

using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static string Name(this ClaimsPrincipal user) => user.Identity!.Name!;
}
