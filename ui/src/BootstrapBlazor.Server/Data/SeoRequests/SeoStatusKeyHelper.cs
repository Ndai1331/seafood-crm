using BootstrapBlazor.Components;

namespace BootstrapBlazor.Server.Data.SeoRequests;

public static class SeoStatusKeyHelper
{
    private static readonly Dictionary<string, string> DefaultCodeLabels =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SEO"] = "Đang SEO",
            ["DUY_TRI_TOP"] = "Duy Trì TOP",
            ["NGUNG_SEO"] = "Ngừng SEO",
            ["KHAC"] = "Khác"
        };

    public static Dictionary<string, string> BuildLabelLookup(IEnumerable<SeoKeyStatusDto> items)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Code))
                continue;

            dict[item.Code] = string.IsNullOrWhiteSpace(item.Name) ? item.Code : item.Name;
        }

        return dict;
    }

    public static Dictionary<string, string> BuildLabelLookup(IEnumerable<SelectedItem<string>> items)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Value))
                continue;

            dict[item.Value] = string.IsNullOrWhiteSpace(item.Text) ? item.Value : item.Text;
        }

        return dict;
    }

    public static string GetLabel(string? code, IReadOnlyDictionary<string, string>? lookup = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "-";

        if (lookup != null && lookup.TryGetValue(code, out var fromLookup) && !string.IsNullOrWhiteSpace(fromLookup))
            return fromLookup;

        if (DefaultCodeLabels.TryGetValue(code, out var fromDefault))
            return fromDefault;

        return code.Replace('_', ' ');
    }
}
