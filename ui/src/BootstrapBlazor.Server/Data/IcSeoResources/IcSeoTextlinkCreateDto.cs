using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Create DTO for IC SEO Textlink resource
/// Required: Domain, Anchor, LinkAnchor
/// </summary>
public class IcSeoTextlinkCreateDto
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("anchor")]
    public string Anchor { get; set; } = string.Empty;

    [JsonPropertyName("linkAnchor")]
    public string LinkAnchor { get; set; } = string.Empty;

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("os")]
    public string? Os { get; set; }

    [JsonPropertyName("orderDate")]
    public DateTime? OrderDate { get; set; }

    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }
}
