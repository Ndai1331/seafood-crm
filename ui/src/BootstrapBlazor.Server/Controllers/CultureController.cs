

using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace BootstrapBlazor.Controllers;

/// <summary>
/// </summary>
[Route("[controller]/[action]")]
public class CultureController : Controller
{
    public IActionResult SetCulture(string culture, string redirectUri)
    {
        if (string.IsNullOrEmpty(culture))
        {
            HttpContext.Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
        }
        else
        {
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)), new CookieOptions()
                {
                    Expires = DateTimeOffset.Now.AddYears(1)
                });
            var lang = culture.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "vi";
            HttpContext.Response.Cookies.Append("sf-lang", lang, new CookieOptions
            {
                Path = "/",
                Expires = DateTimeOffset.Now.AddYears(1),
                IsEssential = true
            });
        }

        return LocalRedirect(redirectUri);
    }

    public IActionResult ResetCulture(string redirectUri)
    {
        HttpContext.Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);

        return LocalRedirect(redirectUri);
    }
}
