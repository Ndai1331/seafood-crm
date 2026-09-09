using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace BootstrapBlazor.Server.Helper;

public static class RedirectChecker
{
    private const int MaxInspectionLength = 128 * 1024;

    private static readonly Regex[] JavaScriptRedirectPatterns =
    [
        new(@"(?:(?:window|document|top)\.)?location(?:\.href)?\s*=\s*[""'](?<url>[^""']+)[""']", RegexOptions.IgnoreCase),
        new(@"(?:(?:window|document)\.)?location\.(?:assign|replace)\(\s*[""'](?<url>[^""']+)[""']\s*\)", RegexOptions.IgnoreCase)
    ];

    public static async Task<(string finalDomain, string logs, int redirectCount, string chain)> CheckRedirectAsync(DomainCheckerDto domain)
    {
        var input = string.IsNullOrWhiteSpace(domain.RequestUrl) ? domain.Domain : domain.RequestUrl;
        var result = await TraceRedirectAsync(input);
        return (StripProtocol(result.FinalUrl), result.Logs, result.RedirectCount, result.Chain);
    }

    public static async Task<RedirectCheckResult> TraceRedirectAsync(string url, int maxRedirects = 10)
    {
        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(35) };
        SetUserAgent(client);
        return await TraceRedirectAsync(url, client, maxRedirects);
    }

    public static async Task<RedirectCheckResult> TraceRedirectAsync(string url, HttpClient client, int maxRedirects = 10)
    {
        var result = new RedirectCheckResult();
        if (!TryNormalizeHttpUrl(url, out var currentUri))
        {
            result.ErrorMessage = "URL không hợp lệ hoặc không dùng giao thức HTTP/HTTPS.";
            result.Logs = result.ErrorMessage;
            return result;
        }

        var visited = new HashSet<string>(StringComparer.Ordinal)
        {
            currentUri.AbsoluteUri
        };

        try
        {
            while (true)
            {
                using var response = await client.GetAsync(currentUri, HttpCompletionOption.ResponseHeadersRead);
                var hop = new RedirectHopDto
                {
                    StepNumber = result.Hops.Count + 1,
                    Url = currentUri.AbsoluteUri,
                    StatusCode = (int)response.StatusCode
                };
                result.Hops.Add(hop);

                Uri? nextUri = null;
                var redirectType = string.Empty;

                if (IsHttpRedirect(response.StatusCode))
                {
                    if (response.Headers.Location is null)
                    {
                        result.ErrorMessage = $"HTTP {(int)response.StatusCode} không có Location header.";
                        break;
                    }

                    if (!TryResolveHttpUrl(currentUri, response.Headers.Location.ToString(), out var resolvedUri))
                    {
                        result.ErrorMessage = $"Location không hợp lệ: {response.Headers.Location}";
                        break;
                    }

                    nextUri = resolvedUri;
                    redirectType = "HTTP";
                }
                else if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var inspectedContent = content[..Math.Min(content.Length, MaxInspectionLength)];
                    if (TryGetClientRedirect(inspectedContent, currentUri, out var resolvedUri, out redirectType))
                    {
                        nextUri = resolvedUri;
                    }
                }

                if (nextUri is null)
                {
                    hop.IsFinal = true;
                    break;
                }

                hop.RedirectType = redirectType;
                hop.NextUrl = nextUri.AbsoluteUri;
                if (!TryContinue(result, visited, nextUri, maxRedirects)) break;
                currentUri = nextUri;
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }

        result.FinalUrl = result.Hops.LastOrDefault(hop => hop.IsFinal)?.Url ?? string.Empty;
        result.Logs = BuildLogs(result);
        return result;
    }

    private static bool TryContinue(
        RedirectCheckResult result,
        HashSet<string> visited,
        Uri nextUri,
        int maxRedirects)
    {
        if (visited.Contains(nextUri.AbsoluteUri))
        {
            result.ErrorMessage = $"Phát hiện vòng lặp redirect tới {nextUri.AbsoluteUri}.";
            return false;
        }

        if (result.RedirectCount >= maxRedirects)
        {
            result.ErrorMessage = $"Đã đạt giới hạn {maxRedirects} lần redirect.";
            return false;
        }

        visited.Add(nextUri.AbsoluteUri);
        result.RedirectCount++;
        return true;
    }

    private static bool TryGetClientRedirect(string content, Uri currentUri, out Uri nextUri, out string redirectType)
    {
        var document = new HtmlDocument();
        document.LoadHtml(content);
        var metaRefresh = document.DocumentNode.SelectNodes("//meta")?
            .FirstOrDefault(node => string.Equals(
                node.GetAttributeValue("http-equiv", string.Empty),
                "refresh",
                StringComparison.OrdinalIgnoreCase));

        if (metaRefresh is not null)
        {
            var value = metaRefresh.GetAttributeValue("content", string.Empty);
            var match = Regex.Match(value, @"(?:^|;)\s*url\s*=\s*(?<url>.+?)\s*$", RegexOptions.IgnoreCase);
            if (match.Success && TryResolveHttpUrl(
                    currentUri,
                    HtmlEntity.DeEntitize(match.Groups["url"].Value).Trim().Trim('"', '\''),
                    out nextUri))
            {
                redirectType = "Meta refresh";
                return true;
            }
        }

        foreach (var pattern in JavaScriptRedirectPatterns)
        {
            var match = pattern.Match(content);
            var target = match.Groups["url"].Value.Replace("\\/", "/", StringComparison.Ordinal);
            if (match.Success && TryResolveHttpUrl(currentUri, target, out nextUri))
            {
                redirectType = "JavaScript heuristic";
                return true;
            }
        }

        nextUri = null!;
        redirectType = string.Empty;
        return false;
    }

    private static bool TryNormalizeHttpUrl(string value, out Uri uri)
    {
        var normalized = value.Trim();
        if (!normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            normalized = $"https://{normalized}";
        }

        return Uri.TryCreate(normalized, UriKind.Absolute, out uri!) && IsHttpUri(uri);
    }

    private static bool TryResolveHttpUrl(Uri currentUri, string target, out Uri uri) =>
        Uri.TryCreate(currentUri, target.Trim(), out uri!) && IsHttpUri(uri);

    private static bool IsHttpUri(Uri uri) =>
        uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
        uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

    private static bool IsHttpRedirect(HttpStatusCode statusCode) => statusCode is
        HttpStatusCode.MultipleChoices or
        HttpStatusCode.MovedPermanently or
        HttpStatusCode.Redirect or
        HttpStatusCode.SeeOther or
        HttpStatusCode.TemporaryRedirect or
        HttpStatusCode.PermanentRedirect;

    private static string BuildLogs(RedirectCheckResult result)
    {
        IEnumerable<string> logs = result.Hops.Select(hop => string.IsNullOrEmpty(hop.NextUrl)
            ? $"{hop.StatusCode} {hop.Url}"
            : $"{hop.StatusCode} {hop.Url} -> {hop.NextUrl} ({hop.RedirectType})");
        if (!string.IsNullOrWhiteSpace(result.ErrorMessage)) logs = logs.Append(result.ErrorMessage);
        return string.Join("|| ", logs);
    }

    private static string StripProtocol(string url)
    {
        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return url[8..].TrimEnd('/');
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) return url[7..].TrimEnd('/');
        return url.TrimEnd('/');
    }

    private static void SetUserAgent(HttpClient client)
    {
        client.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");
    }
}
