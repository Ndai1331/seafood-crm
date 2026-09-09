namespace BootstrapBlazor.Server.Data.SeoDomainInventories;

/// <summary>
/// Visual tokens matching Google Sheet "1. DOMAIN INVENTORY" (header bands + cell pills).
/// </summary>
public static class SeoDomainInventorySheetStyle
{
    // Header bands
    public const string HeaderRed = "#E53935";
    public const string HeaderBlue = "#1E88E5";
    public const string HeaderMaroon = "#8B0000";
    public const string HeaderBlack = "#212121";
    public const string HeaderNeutral = "#607D8B";

    public static string PillStyle(string bg, string fg) =>
        $"background:{bg};color:{fg};";

    public static string ForPic() => PillStyle("#6D4C41", "#FFFFFF");

    public static string ForTeam() => PillStyle("#5E35B1", "#FFFFFF");

    public static string ForClassification(string? value)
    {
        var t = (value ?? "").Trim();
        if (t.Equals("Aged", StringComparison.OrdinalIgnoreCase))
            return PillStyle("#0D47A1", "#FFFFFF");
        if (t.Equals("New", StringComparison.OrdinalIgnoreCase))
            return PillStyle("#1565C0", "#FFFFFF");
        return PillStyle("#1565C0", "#FFFFFF");
    }

    public static string ForNhomKey() => PillStyle("#FFE0B2", "#B71C1C");

    public static string ForMarket(string? value)
    {
        var t = (value ?? "").Trim().ToUpperInvariant();
        if (t is "VN" or "VIỆT NAM" or "VIET NAM")
            return PillStyle("#B71C1C", "#FFFFFF");
        if (t is "TH" or "THÁI" or "THAI")
            return PillStyle("#C62828", "#FFFFFF");
        return PillStyle("#B71C1C", "#FFFFFF");
    }

    public static string ForDifficulty(string? value)
    {
        var t = (value ?? "").Trim();
        if (Contains(t, "Siêu Khó") || Contains(t, "Rất Khó") || Contains(t, "Khó cao"))
            return PillStyle("#FFCDD2", "#B71C1C");
        if (Contains(t, "TB khó") || Contains(t, "TB Kho"))
            return PillStyle("#FFE0B2", "#BF360C");
        if (Contains(t, "Khó"))
            return PillStyle("#FFCCBC", "#BF360C");
        if (Contains(t, "Dễ") && !Contains(t, "Rất Dễ"))
            return PillStyle("#FFCDD2", "#B71C1C");
        if (Contains(t, "Rất Dễ"))
            return PillStyle("#ECEFF1", "#424242");
        return PillStyle("#FFF3E0", "#E65100");
    }

    public static string ForUsagePurpose(string? value)
    {
        var t = (value ?? "").Trim();
        if (Contains(t, "Redirect"))
            return PillStyle("#FFF9C4", "#E65100");
        if (Contains(t, "PBN"))
            return PillStyle("#E1BEE7", "#6A1B9A");
        if (Contains(t, "Pub"))
            return PillStyle("#BBDEFB", "#0D47A1");
        return PillStyle("#FFF8E1", "#F57C00");
    }

    private static bool Contains(string haystack, string needle) =>
        haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
