namespace Template.Server.Components.Authentication;

using System.Security.Claims;

public sealed class LoginManager
{
    private readonly ILoginProvider provider;

    public LoginManager(ILoginProvider provider)
    {
        this.provider = provider;
    }

    public async Task<bool> LoginAsync(string id, string password)
    {
        // TODO custom
        if (id != password)
        {
            return false;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, id)
        };
        if (id == "admin")
        {
            claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
        }

        var identify = new ClaimsIdentity(claims, "custom");
        await provider.LoginAsync(identify);

        return true;
    }

    public Task LogoutAsync() => provider.LogoutAsync();
}
