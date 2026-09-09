using Newtonsoft.Json;

namespace BootstrapBlazor.Server.Data.GeoBlock
{
    // === Request Models ===

    public class GeoBlockStartCheckRequest
    {
        [JsonProperty("urls")] public List<string> Urls { get; set; } = new();
        [JsonProperty("countries")] public List<string> Countries { get; set; } = new();
        [JsonProperty("user_agents")] public List<string> UserAgents { get; set; } = new() { "chrome" };
    }

    public class GeoBlockProxyPreviewRequest
    {
        [JsonProperty("url")] public string Url { get; set; } = "";
        [JsonProperty("country")] public string Country { get; set; } = "";
        [JsonProperty("user_agent")] public string UserAgent { get; set; } = "chrome";
    }

    // === Response Models ===

    public class GeoBlockStartCheckResponse
    {
        [JsonProperty("job_id")] public string JobId { get; set; } = "";
        [JsonProperty("total")] public int Total { get; set; }
    }

    public class GeoBlockJobStatusResponse
    {
        [JsonProperty("job_id")] public string JobId { get; set; } = "";
        [JsonProperty("status")] public string Status { get; set; } = "";
        [JsonProperty("progress")] public double Progress { get; set; }
        [JsonProperty("total")] public int Total { get; set; }
        [JsonProperty("completed")] public int Completed { get; set; }
        [JsonProperty("total_urls")] public int TotalUrls { get; set; }
        [JsonProperty("completed_urls")] public int CompletedUrls { get; set; }
        [JsonProperty("current_country")] public string? CurrentCountry { get; set; }
        [JsonProperty("results")] public List<GeoBlockJobResult> Results { get; set; } = new();
    }

    public class GeoBlockJobResult
    {
        [JsonProperty("url")] public string Url { get; set; } = "";
        [JsonProperty("country")] public string Country { get; set; } = "";
        [JsonProperty("user_agent")] public string UserAgent { get; set; } = "chrome";
        [JsonProperty("status_code")] public int? StatusCode { get; set; }
        [JsonProperty("final_url")] public string? FinalUrl { get; set; }
        [JsonProperty("is_blocked")] public bool IsBlocked { get; set; }
        [JsonProperty("block_reason")] public string? BlockReason { get; set; }
        [JsonProperty("block_source")] public string? BlockSource { get; set; } // 'cloudflare' | 'site_admin' | null
        [JsonProperty("content_blocked")] public bool ContentBlocked { get; set; }
        [JsonProperty("content_block_reason")] public string? ContentBlockReason { get; set; }
        [JsonProperty("error")] public string? Error { get; set; }
        [JsonProperty("detected_ip")] public string? DetectedIp { get; set; }
        [JsonProperty("detected_country")] public string? DetectedCountry { get; set; }
        [JsonProperty("response_time")] public double? ResponseTime { get; set; }
        [JsonProperty("redirects")] public int Redirects { get; set; }
        [JsonProperty("redirect_chain")] public List<GeoBlockRedirectHop>? RedirectChain { get; set; }
        [JsonProperty("method")] public string? Method { get; set; }
        [JsonProperty("proxy_used")] public string? ProxyUsed { get; set; }
        [JsonProperty("response_headers")] public Dictionary<string, string>? ResponseHeaders { get; set; }
        [JsonProperty("response_body")] public string? ResponseBody { get; set; }
        [JsonProperty("is_accessible")] public bool IsAccessible { get; set; }
    }

    public class GeoBlockRedirectHop
    {
        [JsonProperty("url")] public string Url { get; set; } = "";
        [JsonProperty("status")] public int StatusCode { get; set; }
        [JsonProperty("location")] public string? Location { get; set; }
    }

    public class GeoBlockJobSummary
    {
        [JsonProperty("job_id")] public string JobId { get; set; } = "";
        [JsonProperty("status")] public string Status { get; set; } = "";
        [JsonProperty("progress")] public double Progress { get; set; }
        [JsonProperty("total")] public int Total { get; set; }
        [JsonProperty("completed")] public int Completed { get; set; }
        [JsonProperty("total_urls")] public int TotalUrls { get; set; }
        [JsonProperty("total_countries")] public int TotalCountries { get; set; }
        [JsonProperty("completed_urls")] public int CompletedUrls { get; set; }
        [JsonProperty("countries")] public List<string> Countries { get; set; } = new();
        [JsonProperty("user_agents")] public List<string> UserAgents { get; set; } = new();
        [JsonProperty("current_country")] public string? CurrentCountry { get; set; }
        [JsonProperty("created_at")] public string? CreatedAt { get; set; }
        [JsonProperty("completed_at")] public string? CompletedAt { get; set; }
    }

    public class GeoBlockJobsListResponse
    {
        [JsonProperty("jobs")] public List<GeoBlockJobSummary> Jobs { get; set; } = new();
        [JsonProperty("total")] public int Total { get; set; }
        [JsonProperty("running")] public int Running { get; set; }
        [JsonProperty("completed")] public int CompletedCount { get; set; }
        [JsonProperty("failed")] public int Failed { get; set; }
        [JsonProperty("cancelled")] public int Cancelled { get; set; }
    }

    public class GeoBlockCountryInfo
    {
        [JsonProperty("name")] public string Name { get; set; } = "";
        [JsonProperty("flag")] public string Flag { get; set; } = "🏳️";
        [JsonProperty("continent")] public string? Continent { get; set; }
    }

    public class GeoBlockSettingsResponse
    {
        [JsonProperty("webshare_api_key_masked")] public string? WebshareApiKeyMasked { get; set; }
        [JsonProperty("countries")] public Dictionary<string, GeoBlockCountryInfo> Countries { get; set; } = new();
        [JsonProperty("use_proxy_server")] public bool UseProxyServer { get; set; }
        [JsonProperty("residential_proxy_mode")] public string? ResidentialProxyMode { get; set; }
        [JsonProperty("use_browser_fallback")] public bool UseBrowserFallback { get; set; }
        [JsonProperty("default_country_codes")] public List<string> DefaultCountryCodes { get; set; } = new();
        [JsonProperty("country_groups")] public Dictionary<string, List<string>>? CountryGroups { get; set; }
        [JsonProperty("custom_user_agents")] public List<Dictionary<string, string>>? CustomUserAgents { get; set; }
        // AI settings
        [JsonProperty("ai_api_key_masked")] public string? AiApiKeyMasked { get; set; }
        [JsonProperty("ai_model")] public string? AiModel { get; set; }
        [JsonProperty("ai_api_base")] public string? AiApiBase { get; set; }
        [JsonProperty("ai_confidence_threshold")] public double? AiConfidenceThreshold { get; set; }
    }

    public class GeoBlockSettingsUpdateRequest
    {
        [JsonProperty("webshare_api_key")] public string? WebshareApiKey { get; set; }
        [JsonProperty("use_proxy_server")] public bool? UseProxyServer { get; set; }
        [JsonProperty("residential_proxy_mode")] public string? ResidentialProxyMode { get; set; }
        [JsonProperty("use_browser_fallback")] public bool? UseBrowserFallback { get; set; }
        [JsonProperty("ai_api_key")] public string? AiApiKey { get; set; }
        [JsonProperty("ai_model")] public string? AiModel { get; set; }
        [JsonProperty("ai_api_base")] public string? AiApiBase { get; set; }
        [JsonProperty("ai_confidence_threshold")] public double? AiConfidenceThreshold { get; set; }
    }

    public class GeoBlockProxyPreviewResponse
    {
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("html")] public string? Html { get; set; }
        [JsonProperty("error")] public string? Error { get; set; }
        [JsonProperty("status_code")] public int? StatusCode { get; set; }
        [JsonProperty("is_blocked")] public bool IsBlocked { get; set; }
        [JsonProperty("block_reason")] public string? BlockReason { get; set; }
        [JsonProperty("block_source")] public string? BlockSource { get; set; }
        [JsonProperty("final_url")] public string? FinalUrl { get; set; }
        [JsonProperty("detected_ip")] public string? DetectedIp { get; set; }
        [JsonProperty("detected_country")] public string? DetectedCountry { get; set; }
        [JsonProperty("response_time")] public int? ResponseTime { get; set; }
    }

    // === Preset Models ===

    public class GeoBlockPreset
    {
        [JsonProperty("name")] public string Name { get; set; } = "";
        [JsonProperty("urls")] public string? Urls { get; set; }
        [JsonProperty("countries")] public List<string>? Countries { get; set; }
        [JsonProperty("user_agents")] public List<string>? UserAgents { get; set; }
    }
}
