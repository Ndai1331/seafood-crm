using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// DTO for IC SEO REF config reference data
/// </summary>
public class IcSeoRefDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

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
    public bool IsActive { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
