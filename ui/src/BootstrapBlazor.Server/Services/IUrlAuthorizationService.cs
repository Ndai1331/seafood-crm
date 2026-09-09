using BootstrapBlazor.Components;

namespace BootstrapBlazor.Server.Services;

/// <summary>
/// Permission-based URL authorization: menu, route guard and default page all
/// derive from PageRegistry + the current user's permissions (my-permissions API).
/// </summary>
public interface IUrlAuthorizationService
{
    /// <summary>Landing page url for the current user (prefers report-seo-performance).</summary>
    Task<string> GetDefaultPageAsync();

    /// <summary>True when the current user may open the url (public urls always pass).</summary>
    Task<bool> IsUrlAllowedAsync(string url);

    /// <summary>Sidebar menu filtered by the current user's permissions.</summary>
    Task<List<MenuItem>> BuildMenusAsync();
}
