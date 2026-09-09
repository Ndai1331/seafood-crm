using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Filter DTO for IC SEO Resource Checks list endpoint (extends FilterPagingBase)
/// </summary>
public class IcSeoResourceCheckFilterDto : FilterPagingBase
{
    [JsonPropertyName("checkType")]
    public string? CheckType { get; set; }

    [JsonPropertyName("resourceId")]
    public long? ResourceId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("os")]
    public string? Os { get; set; }

    [JsonPropertyName("errorType")]
    public string? ErrorType { get; set; }

    [JsonPropertyName("fromDate")]
    public DateTime? FromDate { get; set; }

    [JsonPropertyName("toDate")]
    public DateTime? ToDate { get; set; }
}
