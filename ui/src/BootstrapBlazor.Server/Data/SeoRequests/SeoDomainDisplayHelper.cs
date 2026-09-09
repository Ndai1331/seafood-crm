namespace BootstrapBlazor.Server.Data.SeoRequests;

/// <summary>
/// Domain display vs storage: UI shows https://…; API/DB stores bare host.
/// </summary>
public static class SeoDomainDisplayHelper
{
    public static string? ParseForStorage(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var value = raw.Trim();
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
            value = value[1..^1].Trim();

        if (value.Length == 0)
            return null;

        if (!value.Contains("://", StringComparison.Ordinal))
            value = "https://" + value;

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            !string.IsNullOrWhiteSpace(uri.Host))
        {
            var host = uri.Host.Trim().TrimEnd('.').ToLowerInvariant();
            if (host.StartsWith("www.", StringComparison.Ordinal))
                host = host[4..];
            return string.IsNullOrWhiteSpace(host) ? null : host;
        }

        var fallback = raw.Trim();
        fallback = StripPrefix(fallback, "https://");
        fallback = StripPrefix(fallback, "http://");
        var cut = fallback.IndexOfAny(['/', '?', '#']);
        if (cut >= 0)
            fallback = fallback[..cut];
        fallback = StripPrefix(fallback, "www.");
        fallback = fallback.Trim().TrimEnd('.').ToLowerInvariant();
        return string.IsNullOrWhiteSpace(fallback) ? null : fallback;
    }

    public static string FormatForDisplay(string? raw)
    {
        var host = ParseForStorage(raw);
        return host == null ? string.Empty : "https://" + host;
    }

    private static string StripPrefix(string value, string prefix) =>
        value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..]
            : value;
}
