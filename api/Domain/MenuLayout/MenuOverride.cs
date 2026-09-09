using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.MenuLayout
{
    // Presentation-only override of sidebar menu order/grouping. PageRegistry.cs (UI) stays the
    // catalog of which pages exist + their permission code; this table only reorders/regroups
    // how they render. Never touches Permission codes or route-guard behavior.
    [Table("menu_overrides")]
    public class MenuOverride
    {
        public int Id { get; set; }

        // "module" or "page".
        [MaxLength(16)]
        public string ItemType { get; set; } = string.Empty;

        // module rows: module Text. page rows: page Url. Unique with ItemType.
        [MaxLength(256)]
        public string ItemKey { get; set; } = string.Empty;

        // page rows only: effective module grouping. Null for module rows.
        [MaxLength(256)]
        public string? ModuleText { get; set; }

        // Ordinal within scope: module order among modules, or page order within its effective module.
        public int SortOrder { get; set; }

        // module rows only: Font Awesome icon class for a custom module (one not in PageRegistry).
        // Null for page rows, and ignored for module rows that match a PageRegistry module (its
        // own icon wins).
        [MaxLength(64)]
        public string? Icon { get; set; }

        // page rows only: admin-chosen sidebar label replacing PageRegistry's Text. Null/blank
        // means "keep the name defined in code" — renaming here never changes the route or the
        // permission code, only what the sidebar prints.
        [MaxLength(256)]
        public string? TextOverride { get; set; }

        // page rows only: runtime show/hide toggle in the sidebar — independent of Permission,
        // does not affect route guard (disabled page stays reachable by direct URL if permitted).
        // Always true for module rows.
        public bool IsEnabled { get; set; } = true;

        [MaxLength(128)]
        public string? UpdatedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
