using BootstrapBlazor.Components;

namespace BootstrapBlazor.Server.Services;

/// <summary>
/// Permission-based implementation over PageRegistry. Replaces the legacy
/// role-hardcoded menu whitelist that was duplicated with PageLayout.razor.cs.
/// </summary>
public class UrlAuthorizationService : IUrlAuthorizationService
{
    private readonly IUserPermissionService _userPermissions;
    private readonly IMenuLayoutProvider _menuLayoutProvider;
    private readonly BootstrapBlazor.Server.Services.I18n.IAppLang? _lang;

    public UrlAuthorizationService(
        IUserPermissionService userPermissions,
        IMenuLayoutProvider menuLayoutProvider,
        BootstrapBlazor.Server.Services.I18n.IAppLang? lang = null)
    {
        _userPermissions = userPermissions;
        _menuLayoutProvider = menuLayoutProvider;
        _lang = lang;
    }

    public async Task<string> GetDefaultPageAsync()
    {
        var (permissions, defaultPageUrl) = await _userPermissions.GetPermissionsWithDefaultAsync();

        var roleDefault = PageRegistry.AllEntries()
            .FirstOrDefault(e => e.Url.Equals(defaultPageUrl, StringComparison.OrdinalIgnoreCase));
        if (roleDefault != null && permissions.Contains(roleDefault.Permission))
        {
            return roleDefault.Url;
        }

        var preferred = PageRegistry.AllEntries()
            .FirstOrDefault(e => e.Url.Equals(PageRegistry.PreferredDefaultUrl, StringComparison.OrdinalIgnoreCase));
        if (preferred != null && permissions.Contains(preferred.Permission))
        {
            return preferred.Url;
        }

        // First visible (non-hidden) page the user can open, in registry order.
        var first = PageRegistry.AllEntries()
            .FirstOrDefault(e => !e.Hidden && permissions.Contains(e.Permission));
        return first?.Url ?? "";
    }

    public async Task<bool> IsUrlAllowedAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        var normalizedUrl = url.TrimStart('/').Split('?')[0].Split('#')[0].Trim();
        if (PageRegistry.PublicUrls.Contains(normalizedUrl))
        {
            return true;
        }

        // Longest-prefix match so a more specific registered route wins over its parent
        // (e.g. domain-price-eval/worker-config has its own permission and must NOT
        // inherit domain-price-eval's; but domain-price-eval/edit/5 falls back to parent).
        var match = PageRegistry.AllEntries()
            .Where(e => normalizedUrl.Equals(e.Url, StringComparison.OrdinalIgnoreCase)
                        || normalizedUrl.StartsWith(e.Url + "/", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Url.Length)
            .FirstOrDefault();

        if (match == null)
        {
            return false;
        }

        return await _userPermissions.HasAsync(match.Permission);
    }

    public async Task<List<MenuItem>> BuildMenusAsync()
    {
        var permissions = await _userPermissions.GetPermissionsAsync();
        var overrides = await _menuLayoutProvider.GetOverridesAsync();
        var modules = MenuOverrideMerger.Apply(PageRegistry.Modules, overrides);
        var enabledMap = MenuOverrideMerger.GetPageEnabledMap(overrides);
        var menus = new List<MenuItem>();

        foreach (var module in modules)
        {
            var items = new List<MenuItem>();
            foreach (var entry in module.Items)
            {
                if (!permissions.Contains(entry.Permission))
                {
                    continue;
                }

                // Presentation only — never affects IsUrlAllowedAsync, so a page stays reachable
                // by direct URL either way as long as permission holds.
                if (!MenuOverrideMerger.IsPageVisible(entry.Url, entry.Hidden, enabledMap))
                {
                    continue;
                }

                var menuItem = new MenuItem { Text = T(entry.LangKey, entry.Text), Icon = entry.Icon, Url = entry.Url };
                if (entry.Children != null)
                {
                    var children = entry.Children
                        .Where(c => !c.Hidden && permissions.Contains(c.Permission))
                        .Select(c => new MenuItem { Text = T(c.LangKey, c.Text), Icon = c.Icon, Url = c.Url })
                        .ToList();
                    if (children.Count > 0)
                    {
                        menuItem.Items = children;
                    }
                }

                items.Add(menuItem);
            }

            if (items.Count > 0)
            {
                menus.Add(new MenuItem { Text = T(module.LangKey, module.Text), Icon = module.Icon, Items = items });
            }
        }

        return menus;
    }

    private string T(string langKey, string fallback)
    {
        if (_lang == null || string.IsNullOrWhiteSpace(langKey)) return fallback;
        var value = _lang[langKey];
        return value == langKey ? fallback : value;
    }
}
