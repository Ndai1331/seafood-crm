using System.Text.Json;
using System.Text.Json.Serialization;
namespace BootstrapBlazor.Server.Data.GoogleSearch;

/// <summary>
/// ScrapingBee Google Search API response
/// </summary>
public class GoogleSearchApiResponse
{
    [JsonPropertyName("meta_data")]
    public GoogleSearchMetaData? MetaData { get; set; }

    [JsonPropertyName("organic_results")]
    public List<GoogleSearchOrganicResult> OrganicResults { get; set; } = new();
}

public class GoogleSearchMetaData
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("number_of_results")]
    public JsonElement? NumberOfResultsElement { get; set; }

    [JsonIgnore]
    public long NumberOfResults 
    { 
        get 
        {
            if (!NumberOfResultsElement.HasValue) return 0;
            var el = NumberOfResultsElement.Value;
            if (el.ValueKind == JsonValueKind.Number) return el.GetInt64();
            if (el.ValueKind == JsonValueKind.String) 
            {
                var str = el.GetString()?.Replace(",", "").Replace(".", "");
                if (long.TryParse(str, out var l)) return l;
            }
            return 0;
        }
    }

    [JsonPropertyName("number_of_organic_results")]
    public int NumberOfOrganicResults { get; set; }

    [JsonPropertyName("number_of_ads")]
    public int NumberOfAds { get; set; }

    [JsonPropertyName("number_of_page")]
    public int NumberOfPage { get; set; }
}

public class GoogleSearchOrganicResult
{
    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("displayed_url")]
    public string DisplayedUrl { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DB model for stored daily Google SERP results
/// </summary>
public class GoogleSearchDailyResult
{
    public long Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public DateTime SearchDate { get; set; }
    public int Position { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string DisplayedUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long TotalResults { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// ScrapingBee API Key entity for multi-key management with priority + failover
/// </summary>
public class ScrapingBeeApiKey
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public int TotalCredits { get; set; } = 1000;
    public int UsedCredits { get; set; }
    public int RemainingCredits => TotalCredits - UsedCredits;
    public int FailCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? LastFailedAt { get; set; }
    public string? LastError { get; set; }
    /// <summary>Last time credits were reset (daily auto-reset or manual)</summary>
    public DateTime? LastResetAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Status of the daily auto-reset job that clears credits and re-enables all keys
/// </summary>
public class ScrapingBeeKeyResetStatus
{
    /// <summary>Whether the daily auto-reset is enabled</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>Period (yyyy-MM) that was last reset</summary>
    public string LastPeriod { get; set; } = string.Empty;
    /// <summary>When the last reset actually ran</summary>
    public DateTime? LastRunAt { get; set; }
    /// <summary>Next scheduled run: 1st day of next month</summary>
    public DateTime NextRunAt { get; set; }
}
