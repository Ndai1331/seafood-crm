namespace BootstrapBlazor.Server.Services;

/// <summary>
/// Pure merge of PageRegistry's default module/order with DB-stored overrides. Never touches
/// Permission codes, Hidden flags or Children — presentation order/grouping/enable-state and
/// sidebar label only (a renamed page keeps its route, permission and guard behavior).
/// Unknown pages (not present in overrides, e.g. added to PageRegistry after the last admin
/// save) always append to their default module so nothing can disappear from the menu. A page
/// override may also point at a module name that does not exist in PageRegistry — this is how an
/// admin-created custom module (see /menu-arrangement "+ Thêm module") survives a merge.
/// </summary>
public static class MenuOverrideMerger
{
    private const int AppendBase = 1_000_000;
    private const string DefaultCustomModuleIcon = "fa-fw fa-solid fa-folder";

    public static IReadOnlyList<PageModule> Apply(IReadOnlyList<PageModule> defaultModules, List<MenuOverrideItem> overrides)
    {
        if (overrides == null || overrides.Count == 0)
        {
            return defaultModules;
        }

        var pageOverride = overrides
            .Where(o => o.ItemType.Equals("page", StringComparison.OrdinalIgnoreCase))
            .GroupBy(o => o.ItemKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var moduleOverride = overrides
            .Where(o => o.ItemType.Equals("module", StringComparison.OrdinalIgnoreCase))
            .GroupBy(o => o.ItemKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (g.First().SortOrder, g.First().Icon), StringComparer.OrdinalIgnoreCase);

        var moduleIconByText = defaultModules.ToDictionary(m => m.Text, m => m.Icon, StringComparer.OrdinalIgnoreCase);

        // Seed every module override (incl. empty custom modules with no pages assigned yet)
        // so they still render as a (possibly empty) drop target after a reload.
        var regrouped = new Dictionary<string, List<(PageEntry Entry, int SortOrder)>>(StringComparer.OrdinalIgnoreCase);
        foreach (var moduleText in moduleOverride.Keys)
        {
            regrouped[moduleText] = new List<(PageEntry, int)>();
        }

        // Only top-level, non-hidden entries participate — hidden entries and children stay
        // attached to their original entry / default module (out of drag-drop scope).
        for (var moduleIndex = 0; moduleIndex < defaultModules.Count; moduleIndex++)
        {
            var module = defaultModules[moduleIndex];
            for (var entryIndex = 0; entryIndex < module.Items.Count; entryIndex++)
            {
                var entry = module.Items[entryIndex];
                if (entry.Hidden)
                {
                    // Keep hidden entries in their original module so guard/whitelist entries never move.
                    AddTo(regrouped, module.Text, entry, AppendBase + entryIndex);
                    continue;
                }

                if (pageOverride.TryGetValue(entry.Url, out var ov))
                {
                    var effectiveModule = string.IsNullOrWhiteSpace(ov.ModuleText) ? module.Text : ov.ModuleText!;
                    AddTo(regrouped, effectiveModule, entry, ov.SortOrder);
                }
                else
                {
                    AddTo(regrouped, module.Text, entry, AppendBase + entryIndex);
                }
            }
        }

        var orderedModuleTexts = regrouped.Keys
            .OrderBy(text => moduleOverride.TryGetValue(text, out var ov)
                ? ov.SortOrder
                : AppendBase + defaultModules.ToList().FindIndex(m => m.Text.Equals(text, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var result = new List<PageModule>();
        foreach (var moduleText in orderedModuleTexts)
        {
            var items = regrouped[moduleText]
                .OrderBy(x => x.SortOrder)
                .Select(x => WithRenamedText(x.Entry, pageOverride))
                .ToList();
            var icon = moduleIconByText.TryGetValue(moduleText, out var registryIcon)
                ? registryIcon
                : moduleOverride.TryGetValue(moduleText, out var ov) && !string.IsNullOrWhiteSpace(ov.Icon)
                    ? ov.Icon!
                    : DefaultCustomModuleIcon;
            result.Add(new PageModule { Text = moduleText, Icon = icon, Items = items });
        }

        return result;
    }

    /// <summary>Page Url → explicit runtime on/off override, if any was saved. Absence means "no
    /// override" — callers decide the default via <see cref="IsPageVisible"/> (Hidden entries
    /// default OFF, normal entries default ON).</summary>
    public static Dictionary<string, bool> GetPageEnabledMap(List<MenuOverrideItem> overrides)
    {
        var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        if (overrides == null)
        {
            return map;
        }

        foreach (var o in overrides.Where(o => o.ItemType.Equals("page", StringComparison.OrdinalIgnoreCase)))
        {
            map[o.ItemKey] = o.IsEnabled;
        }

        return map;
    }

    /// <summary>Effective sidebar visibility for one page: an explicit override always wins;
    /// otherwise Hidden PageRegistry entries (whitelist-only, e.g. ETL Management's pages) default
    /// to OFF and normal entries default to ON. Never affects permission/route-guard.</summary>
    public static bool IsPageVisible(string url, bool hiddenByDefault, Dictionary<string, bool> enabledMap)
        => enabledMap.TryGetValue(url, out var enabled) ? enabled : !hiddenByDefault;

    /// <summary>Swap in the admin-chosen sidebar label when one was saved for this page. Copies the
    /// entry so PageRegistry's own (static, process-wide) instance is never mutated; Url, Permission,
    /// Hidden and Children are carried over untouched — a rename is presentation only.</summary>
    private static PageEntry WithRenamedText(PageEntry entry, Dictionary<string, MenuOverrideItem> pageOverride)
    {
        if (!pageOverride.TryGetValue(entry.Url, out var ov)
            || string.IsNullOrWhiteSpace(ov.TextOverride)
            || ov.TextOverride.Trim().Equals(entry.Text, StringComparison.Ordinal))
        {
            return entry;
        }

        return new PageEntry
        {
            Url = entry.Url,
            Text = ov.TextOverride.Trim(),
            Icon = entry.Icon,
            Permission = entry.Permission,
            Hidden = entry.Hidden,
            Children = entry.Children,
        };
    }

    private static void AddTo(Dictionary<string, List<(PageEntry, int)>> regrouped, string moduleText, PageEntry entry, int sortOrder)
    {
        if (!regrouped.TryGetValue(moduleText, out var list))
        {
            list = new List<(PageEntry, int)>();
            regrouped[moduleText] = list;
        }
        list.Add((entry, sortOrder));
    }
}
