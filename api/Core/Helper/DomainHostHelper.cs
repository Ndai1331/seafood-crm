namespace Core.Helper
{
    /// <summary>
    /// Lenient host extraction for user-pasted domain blobs (lookup/search input).
    ///
    /// Deliberately separate from DomainPurchaseRequestRules.NormalizeDomain: that one
    /// validates before persisting and rejects anything not a real DNS host. Here the
    /// input is a search term — a value we cannot resolve is still worth reporting back
    /// as "không tìm thấy" instead of being silently dropped as invalid.
    /// </summary>
    public static class DomainHostHelper
    {
        /// <summary>Strip scheme, path, port and the www prefix; lowercase the rest.</summary>
        public static string NormalizeHost(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var value = raw.Trim();
            if (Uri.TryCreate(value.Contains("://", StringComparison.Ordinal) ? value : "https://" + value,
                    UriKind.Absolute, out var uri))
            {
                value = uri.IdnHost;
            }
            else
            {
                value = value.Split('/', '?', '#')[0];
            }

            value = value.Trim().TrimEnd('.').ToLowerInvariant();
            if (value.StartsWith("www.", StringComparison.Ordinal))
            {
                value = value[4..];
            }

            return value;
        }

        /// <summary>
        /// Split a pasted blob (newlines, commas, semicolons, tabs, spaces) into distinct hosts,
        /// preserving the order the user typed them so results read back the same way.
        /// </summary>
        public static List<string> ParseHostList(string? raw, int maxItems = 200)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return result;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var tokens = raw.Split(['\n', '\r', ',', ';', '\t', ' '], StringSplitOptions.RemoveEmptyEntries
                                                                      | StringSplitOptions.TrimEntries);
            foreach (var token in tokens)
            {
                var host = NormalizeHost(token);
                if (host.Length == 0 || !seen.Add(host))
                {
                    continue;
                }

                result.Add(host);
                if (result.Count >= maxItems)
                {
                    break;
                }
            }

            return result;
        }
    }
}
