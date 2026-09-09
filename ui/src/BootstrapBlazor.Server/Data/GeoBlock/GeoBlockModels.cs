using Newtonsoft.Json;

namespace BootstrapBlazor.Server.Data.GeoBlock
{
    public class GeoBlockCountry
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("countryCode")] public string CountryCode { get; set; } = string.Empty;
        [JsonProperty("countryName")] public string CountryName { get; set; } = string.Empty;
        [JsonProperty("isDefault")] public bool IsDefault { get; set; }
        [JsonProperty("sortOrder")] public int SortOrder { get; set; }
    }

    public class GeoBlockDomain
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("domain")] public string Domain { get; set; } = string.Empty;
        [JsonProperty("note")] public string? Note { get; set; }
        [JsonProperty("isActive")] public bool IsActive { get; set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public DateTime UpdatedAt { get; set; }
    }

    public class GeoBlockProxy
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("countryCode")] public string CountryCode { get; set; } = string.Empty;
        [JsonProperty("countryName")] public string CountryName { get; set; } = string.Empty;
        [JsonProperty("proxyType")] public string ProxyType { get; set; } = "static";
        [JsonProperty("proxyAddress")] public string ProxyAddress { get; set; } = string.Empty;
        [JsonProperty("proxyProtocol")] public string ProxyProtocol { get; set; } = "http";
        [JsonProperty("isActive")] public bool IsActive { get; set; }
        [JsonProperty("lastCheckedAt")] public DateTime? LastCheckedAt { get; set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public DateTime UpdatedAt { get; set; }
    }

    public class GeoBlockCheckResult
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("scanId")] public string ScanId { get; set; } = string.Empty;
        [JsonProperty("domainId")] public long DomainId { get; set; }
        [JsonProperty("domain")] public string Domain { get; set; } = string.Empty;
        [JsonProperty("countryCode")] public string CountryCode { get; set; } = string.Empty;
        [JsonProperty("countryName")] public string CountryName { get; set; } = string.Empty;
        [JsonProperty("httpStatusCode")] public int? HttpStatusCode { get; set; }
        [JsonProperty("isBlocked")] public bool IsBlocked { get; set; }
        [JsonProperty("isCloudflare")] public bool IsCloudflare { get; set; }
        [JsonProperty("cloudflareType")] public string? CloudflareType { get; set; }
        [JsonProperty("responseTimeMs")] public int? ResponseTimeMs { get; set; }
        [JsonProperty("errorMessage")] public string? ErrorMessage { get; set; }
        [JsonProperty("proxyId")] public long? ProxyId { get; set; }
        [JsonProperty("checkedAt")] public DateTime CheckedAt { get; set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }
    }

    public class GeoBlockReportRow
    {
        [JsonProperty("domain")] public string Domain { get; set; } = string.Empty;
        [JsonProperty("domainId")] public long DomainId { get; set; }
        [JsonProperty("countryResults")] public List<GeoBlockReportCountryResult> CountryResults { get; set; } = new();
    }

    public class GeoBlockReportCountryResult
    {
        [JsonProperty("countryCode")] public string CountryCode { get; set; } = string.Empty;
        [JsonProperty("countryName")] public string CountryName { get; set; } = string.Empty;
        [JsonProperty("httpStatusCode")] public int? HttpStatusCode { get; set; }
        [JsonProperty("isBlocked")] public bool IsBlocked { get; set; }
        [JsonProperty("isCloudflare")] public bool IsCloudflare { get; set; }
        [JsonProperty("cloudflareType")] public string? CloudflareType { get; set; }
        [JsonProperty("responseTimeMs")] public int? ResponseTimeMs { get; set; }
        [JsonProperty("errorMessage")] public string? ErrorMessage { get; set; }
    }
}
