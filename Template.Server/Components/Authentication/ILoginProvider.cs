namespace Template.Server.Components.Authentication;

using System.Security.Claims;

public interface ILoginProvider
{
    Task LoginAsync(ClaimsIdentity identity);

    Task LogoutAsync();
}
