using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Data;

namespace BootstrapBlazor.Server.Data.SeoRequests;

public static class SeoColoredOptionHelper
{
    // Semantic text colors — no background
    public const string ColorPrimary = "#0d6efd";
    public const string ColorSuccess = "#198754";
    public const string ColorDanger = "#dc3545";
    public const string ColorWarning = "#fd7e14";
    public const string ColorInfo = "#0dcaf0";
    public const string ColorNeutral = "#6c757d";
    public const string ColorPurple = "#6f42c1";
    public const string ColorTeal = "#20c997";

    private static readonly string[] TextPalette =
    [
        ColorPrimary,
        ColorSuccess,
        ColorWarning,
        ColorPurple,
        ColorTeal,
        ColorInfo,
        "#d63384",
        "#6610f2",
        "#0aa2c0",
        "#e65100",
        ColorNeutral,
        ColorDanger
    ];

    private static readonly Dictionary<string, string> StatusKeyTextColors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Đang SEO"] = ColorPrimary,
            ["Duy Trì TOP"] = ColorSuccess,
            ["Duy tri TOP"] = ColorSuccess,
            ["Ngừng SEO"] = ColorDanger,
            ["Ngung SEO"] = ColorDanger,
            ["Khác"] = ColorNeutral
        };

    private static readonly Dictionary<string, string> DifficultyTextColors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Rất Dễ"] = ColorSuccess,
            ["Dễ"] = ColorSuccess,
            ["TB khó"] = ColorWarning,
            ["Khó"] = ColorDanger,
            ["Khó cao"] = ColorDanger,
            ["Rất Khó"] = ColorDanger,
            ["Siêu Khó"] = ColorDanger
        };

    private static readonly Dictionary<string, string> CostTypeTextColors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["textlink"] = ColorSuccess,
            ["entity"] = ColorWarning,
            ["link"] = "#795548",
            ["link p"] = ColorDanger,
            ["link pub"] = ColorDanger,
            ["guest post"] = ColorPrimary,
            ["banner"] = ColorWarning,
            ["pbn"] = ColorPurple
        };

    private static readonly Dictionary<string, string> MarketTextColors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [SeoMarkets.Vn] = ColorPrimary,
            [SeoMarkets.Th] = ColorDanger
        };

    private static readonly Dictionary<string, string> UsagePurposeTextColors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Redirect"] = ColorSuccess,
            ["Pub Seo"] = ColorPrimary,
            ["PBN"] = ColorPurple,
            ["Khác"] = ColorNeutral
        };

    private static readonly Dictionary<string, string> DomainClassificationTextColors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [SeoDomainClassifications.New] = ColorPrimary,
            [SeoDomainClassifications.Aged] = ColorWarning
        };

    public static List<ColoredSelectOption> FromSelectedItems(
        IEnumerable<SelectedItem<string>> items,
        string category = "default")
    {
        var list = items.ToList();
        var result = new List<ColoredSelectOption>(list.Count);

        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            result.Add(CreateOption(
                item.Value ?? string.Empty,
                item.Text,
                ResolveTextColor(item.Value, item.Text, category, i)));
        }

        return result;
    }

    public static List<ColoredSelectOption> FromCostTypes(IEnumerable<CostTypeDto> costTypes)
    {
        var list = costTypes
            .OrderBy(x => x.Name ?? x.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var result = new List<ColoredSelectOption>(list.Count);

        for (var i = 0; i < list.Count; i++)
        {
            var ct = list[i];
            var label = ct.Name ?? ct.Code ?? ct.Id.ToString();
            result.Add(CreateOption(
                ct.Id.ToString(),
                label,
                ResolveCostTypeTextColor(label, ct.Code, i)));
        }

        return result;
    }

    public static ColoredSelectOption? FindOption(IEnumerable<ColoredSelectOption> options, string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return options.FirstOrDefault(x => x.Value == value);
    }

    private static ColoredSelectOption CreateOption(string value, string text, string textColor) =>
        new()
        {
            Value = value,
            Text = text,
            BgColor = "transparent",
            TextColor = textColor
        };

    private static string ResolveTextColor(string? value, string text, string category, int index)
    {
        if (category == "status-key")
            return MatchByLabelOrValue(text, value, StatusKeyTextColors, index);

        if (category == "difficulty")
            return MatchByLabelOrValue(text, value, DifficultyTextColors, index);

        if (category == "market" && value != null && MarketTextColors.TryGetValue(value, out var market))
            return market;

        if (category == "usage" && value != null && UsagePurposeTextColors.TryGetValue(value, out var usage))
            return usage;

        if (category == "domain-classification" && value != null &&
            DomainClassificationTextColors.TryGetValue(value, out var domainClass))
            return domainClass;

        return TextPalette[StableIndex(value ?? text, index) % TextPalette.Length];
    }

    private static string ResolveCostTypeTextColor(string label, string code, int index)
    {
        foreach (var key in CostTypeTextColors.Keys)
        {
            if (label.Contains(key, StringComparison.OrdinalIgnoreCase) ||
                code.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return CostTypeTextColors[key];
            }
        }

        return TextPalette[StableIndex($"{label}|{code}", index) % TextPalette.Length];
    }

    private static string MatchByLabelOrValue(
        string text,
        string? value,
        Dictionary<string, string> map,
        int index)
    {
        if (map.TryGetValue(text, out var byText))
            return byText;

        if (!string.IsNullOrEmpty(value) && map.TryGetValue(value, out var byValue))
            return byValue;

        foreach (var kvp in map)
        {
            if (text.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        return TextPalette[StableIndex(text, index) % TextPalette.Length];
    }

    private static int StableIndex(string key, int fallbackIndex)
    {
        if (string.IsNullOrEmpty(key))
            return fallbackIndex;

        unchecked
        {
            var hash = 0;
            foreach (var ch in key)
                hash = (hash * 31) + ch;

            return Math.Abs(hash);
        }
    }
}
