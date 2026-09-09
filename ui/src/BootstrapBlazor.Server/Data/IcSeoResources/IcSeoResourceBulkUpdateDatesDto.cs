using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// DTO for bulk updating dates, OS, and PIC of IC SEO TL/GP resources.
/// Null fields are not updated (partial update semantics).
/// Matches API Contract: BulkUpdateDatesDto
/// </summary>
public class IcSeoResourceBulkUpdateDatesDto
{
    [JsonPropertyName("ids")]
    public List<long> Ids { get; set; } = new();

    [JsonPropertyName("orderDate")]
    public DateTime? OrderDate { get; set; }

    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }
}
