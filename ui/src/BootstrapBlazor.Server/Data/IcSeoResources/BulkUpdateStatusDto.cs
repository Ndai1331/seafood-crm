using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// DTO for bulk updating active status of IC SEO Resources
/// Matches API Contract: BulkUpdateActiveStatusDto
/// </summary>
public class BulkUpdateStatusDto
{
    [JsonPropertyName("ids")]
    public List<long> Ids { get; set; } = new();

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}
