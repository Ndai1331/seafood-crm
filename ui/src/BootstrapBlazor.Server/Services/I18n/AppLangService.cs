using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.JSInterop;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace BootstrapBlazor.Server.Services.I18n;

public sealed class AppLangService : IAppLang
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _http;
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> Cache = new();
    private const string CookieName = "sf-lang";

    public AppLangService(IWebHostEnvironment env, IHttpContextAccessor http)
    {
        _env = env;
        _http = http;
    }

    public string Current
    {
        get
        {
            var cookie = _http.HttpContext?.Request.Cookies[CookieName];
            if (cookie is "en" or "en-US") return "en";
            var cultureCookie = _http.HttpContext?.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(cultureCookie) && cultureCookie.Contains("en", StringComparison.OrdinalIgnoreCase))
                return "en";
            return "vi";
        }
    }

    public string this[string code]
    {
        get
        {
            var map = Load(Current);
            if (map.TryGetValue(code, out var value) && !string.IsNullOrEmpty(value))
                return value;
            var fallback = Load("vi");
            return fallback.TryGetValue(code, out var vi) && !string.IsNullOrEmpty(vi) ? vi : code;
        }
    }

    public Task SetLangAsync(string lang)
    {
        var normalized = lang.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "vi";
        var culture = normalized == "en" ? "en-US" : "vi-VN";
        _http.HttpContext?.Response.Cookies.Append(CookieName, normalized, new CookieOptions
        {
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true
        });
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = new CultureInfo(culture);
        return Task.CompletedTask;
    }

    private Dictionary<string, string> Load(string lang)
    {
        return Cache.GetOrAdd(lang, key =>
        {
            var path = Path.Combine(_env.WebRootPath, "locales", $"{key}.json");
            if (!File.Exists(path)) return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var flat = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Flatten(doc.RootElement, "", flat);
            return flat;
        });
    }

    private static void Flatten(JsonElement el, string prefix, Dictionary<string, string> target)
    {
        if (el.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in el.EnumerateObject())
            {
                var next = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                Flatten(prop.Value, next, target);
            }
            return;
        }

        if (el.ValueKind == JsonValueKind.String)
            target[prefix] = el.GetString() ?? "";
    }
}
