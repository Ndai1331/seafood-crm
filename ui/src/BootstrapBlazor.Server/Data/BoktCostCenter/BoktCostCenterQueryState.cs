using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;

namespace BootstrapBlazor.Server.Data.BoktCostCenter;

/// <summary>
/// Serializes/deserializes the cost-center page filter into URL query string.
/// Used so the tracing detail page can navigate away and the list page can restore
/// the exact same filter state when the user navigates back.
/// </summary>
public static class BoktCostCenterQueryState
{
    /// <summary>Build a query string ("?k=v&...") from the filter, omitting empty values.</summary>
    public static string ToQueryString(BoktCostCenterFilterDto filter)
    {
        var values = new Dictionary<string, string?>
        {
            ["from"] = filter.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["to"] = filter.ToDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["q"] = filter.FilterText,
            ["month"] = filter.Month?.ToString(CultureInfo.InvariantCulture),
            ["cost"] = filter.CostType,
            ["team"] = filter.Team,
            ["pic"] = filter.Pic,
            ["brand"] = filter.Brand
        };

        var query = new Dictionary<string, string?>();
        foreach (var kv in values)
        {
            if (!string.IsNullOrWhiteSpace(kv.Value))
                query[kv.Key] = kv.Value;
        }

        return QueryHelpers.AddQueryString(string.Empty, query);
    }

    /// <summary>
    /// Restore filter fields from a URI's query string. Returns true if any filter key was present,
    /// so callers can decide whether to override their defaults.
    /// </summary>
    public static bool TryRestore(string uri, BoktCostCenterFilterDto filter, out DateTime? from, out DateTime? to)
    {
        from = null;
        to = null;

        var queryIndex = uri.IndexOf('?');
        if (queryIndex < 0)
            return false;

        var parsed = QueryHelpers.ParseQuery(uri[queryIndex..]);
        var matched = false;

        if (parsed.TryGetValue("from", out var fromRaw)
            && DateTime.TryParse(fromRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromValue))
        {
            from = fromValue;
            filter.FromDate = fromValue;
            matched = true;
        }

        if (parsed.TryGetValue("to", out var toRaw)
            && DateTime.TryParse(toRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var toValue))
        {
            to = toValue;
            filter.ToDate = toValue;
            matched = true;
        }

        if (parsed.TryGetValue("q", out var q) && !string.IsNullOrWhiteSpace(q))
        {
            filter.FilterText = q;
            matched = true;
        }

        if (parsed.TryGetValue("month", out var monthRaw)
            && int.TryParse(monthRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var month))
        {
            filter.Month = month;
            matched = true;
        }

        // supplier / domain đã bỏ khỏi thanh lọc (2026-08-14) vì trùng với Tìm nhanh.
        // Không khôi phục chúng từ URL nữa: URL cũ sẽ lọc âm thầm mà không có ô nào trên màn
        // hình cho thấy vì sao danh sách bị thu hẹp. PIC thì đã đưa trở lại thanh lọc nên khôi
        // phục được — có ô hiển thị đúng giá trị.
        if (parsed.TryGetValue("pic", out var pic) && !string.IsNullOrWhiteSpace(pic))
        {
            filter.Pic = pic;
            matched = true;
        }

        if (parsed.TryGetValue("brand", out var brand) && !string.IsNullOrWhiteSpace(brand))
        {
            filter.Brand = brand;
            matched = true;
        }

        if (parsed.TryGetValue("cost", out var cost) && !string.IsNullOrWhiteSpace(cost))
        {
            filter.CostType = cost;
            matched = true;
        }

        if (parsed.TryGetValue("team", out var team) && !string.IsNullOrWhiteSpace(team))
        {
            filter.Team = team;
            matched = true;
        }

        return matched;
    }
}
