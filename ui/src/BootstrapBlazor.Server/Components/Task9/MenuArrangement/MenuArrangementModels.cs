namespace BootstrapBlazor.Server.Components.Task9;

/// <summary>One page card in the arranger (top-level PageRegistry entries — both visible and
/// Hidden/whitelist-only). Hidden-by-default entries (e.g. ETL Management's pages) are pinned to
/// their original module — MenuOverrideMerger never reassigns them — so the UI disables their
/// move/reassign buttons; only the Enabled toggle applies to them.</summary>
public class EditPage
{
    public required string Url { get; init; }

    // Sidebar label as currently rendered: the admin's rename when one is saved, else DefaultText.
    public required string Text { get; set; }

    // PageRegistry's own label — kept so the arranger can offer "về tên gốc" and so a rename that
    // merely retypes the original is stored as "no override" instead of pinning a duplicate.
    public required string DefaultText { get; init; }
    public string Icon { get; init; } = "";

    // Runtime show/hide in the sidebar — independent of Permission (see MenuOverrideMerger.GetPageEnabledMap).
    // Defaults to PageRegistry's Hidden flag inverted (Hidden entries start OFF, matching legacy
    // whitelist-only behavior) — admin can flip a Hidden entry ON to surface it on the real menu.
    public bool Enabled { get; set; } = true;

    // True for PageRegistry entries with Hidden=true — move/reassign buttons are disabled for these.
    public bool IsHiddenByDefault { get; init; }
}

/// <summary>One module column in the arranger — may be a PageRegistry default module or a
/// custom one created via "+ Thêm module" (persisted as a "module"-type override row).</summary>
public class EditRoleDefaultPage
{
    public int RoleId { get; init; }
    public required string RoleName { get; init; }
    public string? DefaultPageUrl { get; set; }
}

public class EditModule
{
    public required string Text { get; init; }
    public string Icon { get; init; } = "";
    public required List<EditPage> Items { get; init; }
}
