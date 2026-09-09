using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Create DTO for IC SEO Guestpost resource
/// Required: Domain, LinkDetail, Anchor, LinkAnchor
/// Optional: Anchor2, LinkAnchor2, Pic, Os, OrderDate, ExpiryDate
/// </summary>
public class IcSeoGuestpostCreateDto
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("linkDetail")]
    public string LinkDetail { get; set; } = string.Empty;

    [JsonPropertyName("anchor")]
    public string Anchor { get; set; } = string.Empty;

    [JsonPropertyName("linkAnchor")]
    public string LinkAnchor { get; set; } = string.Empty;

    [JsonPropertyName("anchor2")]
    public string? Anchor2 { get; set; }

    [JsonPropertyName("linkAnchor2")]
    public string? LinkAnchor2 { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("os")]
    public string? Os { get; set; }

    [JsonPropertyName("orderDate")]
    public DateTime? OrderDate { get; set; }

    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }
}
