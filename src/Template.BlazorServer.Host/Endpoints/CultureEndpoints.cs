namespace Template.BlazorServer.Host.Endpoints;

using Microsoft.AspNetCore.Localization;

public static class CultureEndpoints
{
    //--------------------------------------------------------------------------------
    // Mapping
    //--------------------------------------------------------------------------------

    public static void MapCultureEndpoints(this WebApplication app)
    {
        app.MapGet("/culture/{name}", HandleChange)
            .RequireAuthorization()
            .WithName("CultureChange")
            .Produces(StatusCodes.Status302Found)
            .Produces(StatusCodes.Status404NotFound);
    }

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

    private static IResult HandleChange(HttpContext context, string name)
    {
        if (name is not (Cultures.Japanese or Cultures.English))
        {
            return TypedResults.NotFound();
        }

        context.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(name)),
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps
            });

        return TypedResults.LocalRedirect("~/textresource/locale");
    }
}
