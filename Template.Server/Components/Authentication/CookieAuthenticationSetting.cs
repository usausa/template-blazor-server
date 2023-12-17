namespace Template.Server.Components.Authentication;

public sealed class CookieAuthenticationSetting
{
    public string AccountKey { get; set; } = "__account";

    public string SecretKey { get; set; } = default!;

    public string Issuer { get; set; } = default!;

    public int Expire { get; set; }
}
