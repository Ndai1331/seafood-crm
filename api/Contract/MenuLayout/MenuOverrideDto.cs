namespace Contract.MenuLayout
{
    public class MenuOverrideDto
    {
        public string ItemType { get; set; } = string.Empty;
        public string ItemKey { get; set; } = string.Empty;
        public string? ModuleText { get; set; }
        public string? TextOverride { get; set; }
        public int SortOrder { get; set; }
        public string? Icon { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    // Full-snapshot save: admin's current in-memory arrangement replaces all stored rows.
    public class MenuLayoutSnapshotDto
    {
        public List<MenuOverrideDto> Items { get; set; } = new();
    }
}
