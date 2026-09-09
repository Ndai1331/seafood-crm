using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// DTO for IC SEO Resource Check record
/// </summary>
public class IcSeoResourceCheckDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("checkType")]
    public string? CheckType { get; set; }

    [JsonPropertyName("resourceId")]
    public long ResourceId { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("anchor")]
    public string? Anchor { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("os")]
    public string? Os { get; set; }

    [JsonPropertyName("checkDate")]
    public DateTime CheckDate { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("proxyUsed")]
    public string? ProxyUsed { get; set; }

    [JsonPropertyName("durationMs")]
    public int? DurationMs { get; set; }

    [JsonPropertyName("errorType")]
    public string? ErrorType { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
