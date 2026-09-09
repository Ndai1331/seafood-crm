using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Create/Update DTO for IC SEO REF config reference data.
/// Id is nullable — null for new rows, set for existing rows during bulk upsert.
/// </summary>
public class IcSeoRefCreateDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("refKey")]
    public string RefKey { get; set; } = string.Empty;

    [JsonPropertyName("refValue")]
    public string? RefValue { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
}
