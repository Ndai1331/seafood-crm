using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Filter DTO for IC SEO Resource list endpoints (extends FilterPagingBase)
/// Used for both textlinks and guestposts get-list endpoints
/// </summary>
public class IcSeoResourceFilterDto : FilterPagingBase
{
    [JsonPropertyName("checkType")]
    public string? CheckType { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("lastStatus")]
    public string? LastStatus { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("orderDateFrom")]
    public DateTime? OrderDateFrom { get; set; }

    [JsonPropertyName("orderDateTo")]
    public DateTime? OrderDateTo { get; set; }

    [JsonPropertyName("expiryDateFrom")]
    public DateTime? ExpiryDateFrom { get; set; }

    [JsonPropertyName("expiryDateTo")]
    public DateTime? ExpiryDateTo { get; set; }

    [JsonPropertyName("fromDate")]
    public DateTime? FromDate { get; set; }

    [JsonPropertyName("toDate")]
    public DateTime? ToDate { get; set; }
}
