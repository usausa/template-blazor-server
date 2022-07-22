namespace Template.Server.Components.Authentication;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;

using Smart.Text;

public sealed class CookieAuthenticationStateProvider : AuthenticationStateProvider, ILoginProvider
{
    private static readonly DateTime ExpireDate = new(1970, 1, 1);

    private static readonly ClaimsPrincipal Anonymous = new();

    private readonly IHttpContextAccessor httpContextAccessor;

    private readonly IJSRuntime jsRuntime;

    private readonly CookieAuthenticationSetting setting;

    private readonly byte[] secretKey;

    private ClaimsPrincipal? cachedPrincipal;

    public CookieAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor, IJSRuntime jsRuntime, IOptions<CookieAuthenticationSetting> setting)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.jsRuntime = jsRuntime;
        this.setting = setting.Value;
        secretKey = HexEncoder.Decode(setting.Value.SecretKey);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (cachedPrincipal is not null)
        {
            return Task.FromResult(new AuthenticationState(cachedPrincipal));
        }

        var principal = LoadAccount();
        if (principal is not null)
        {
            cachedPrincipal = principal;
            return Task.FromResult(new AuthenticationState(principal));
        }

        return Task.FromResult(new AuthenticationState(Anonymous));
    }

    public async Task LoginAsync(ClaimsIdentity identity)
    {
        await SaveAccountAsync(identity).ConfigureAwait(false);

        cachedPrincipal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(cachedPrincipal)));
    }

    public async Task LogoutAsync()
    {
        cachedPrincipal = null;
        await UpdateCookie(string.Empty, ExpireDate).ConfigureAwait(false);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private async Task SaveAccountAsync(ClaimsIdentity identity)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Expires = DateTime.UtcNow.AddDays(setting.Expire),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature),
            Audience = setting.Issuer,
            Issuer = setting.Issuer
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var value = tokenHandler.WriteToken(token);

        await UpdateCookie(value, DateTime.Now.AddMinutes(setting.Expire)).ConfigureAwait(false);
    }

    private ClaimsPrincipal? LoadAccount()
    {
        var value = httpContextAccessor.HttpContext?.Request.Cookies[setting.AccountKey];
        if (String.IsNullOrEmpty(value))
        {
            return null;
        }

        try
        {
            var parameter = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidAudience = setting.Issuer,
                ValidIssuer = setting.Issuer
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(value, parameter, out var validatedToken);
            if (validatedToken.ValidTo < DateTime.UtcNow)
            {
                return null;
            }

            return principal;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    private async Task UpdateCookie(string value, DateTime expire)
    {
        await jsRuntime.InvokeVoidAsync("eval", $"document.cookie = \"{setting.AccountKey}={value}; expires={expire.ToUniversalTime():R}\"").ConfigureAwait(false);
    }
}
