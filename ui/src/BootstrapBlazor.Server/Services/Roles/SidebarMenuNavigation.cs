using BootstrapBlazor.Components;

namespace BootstrapBlazor.Server.Services;

/// <summary>Resolves whether a sidebar menu node should navigate directly.</summary>
public static class SidebarMenuNavigation
{
    /// <summary>Returns the node URL, or the only leaf child's URL for singleton modules.</summary>
    public static string? ResolveDirectUrl(MenuItem menu)
    {
        if (!string.IsNullOrWhiteSpace(menu.Url))
        {
            return menu.Url;
        }

        var children = menu.Items?.ToList();
        return children is { Count: 1 } && children[0].Items?.Any() != true
            ? children[0].Url
            : null;
    }
}
