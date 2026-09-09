using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Dashboard summary DTO for IC SEO Resources
/// </summary>
public class IcSeoResourceDashboardDto
{
    [JsonPropertyName("totalResources")]
    public int TotalResources { get; set; }

    [JsonPropertyName("activeResources")]
    public int ActiveResources { get; set; }

    [JsonPropertyName("okCount")]
    public int OkCount { get; set; }

    [JsonPropertyName("missingCount")]
    public int MissingCount { get; set; }

    [JsonPropertyName("errorCount")]
    public int ErrorCount { get; set; }

    [JsonPropertyName("uncheckedCount")]
    public int UncheckedCount { get; set; }

    [JsonPropertyName("tlTotal")]
    public int TlTotal { get; set; }

    [JsonPropertyName("gpTotal")]
    public int GpTotal { get; set; }
}

/// <summary>
/// Statistics by PIC (Person In Charge)
/// </summary>
public class IcSeoResourceStatsByPicDto
{
    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("checkType")]
    public string? CheckType { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("okCount")]
    public int OkCount { get; set; }

    [JsonPropertyName("errorCount")]
    public int ErrorCount { get; set; }

    [JsonPropertyName("missingCount")]
    public int MissingCount { get; set; }
}

/// <summary>
/// Statistics by OS
/// </summary>
public class IcSeoResourceStatsByOsDto
{
    [JsonPropertyName("os")]
    public string? Os { get; set; }

    [JsonPropertyName("checkType")]
    public string? CheckType { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("okCount")]
    public int OkCount { get; set; }

    [JsonPropertyName("errorCount")]
    public int ErrorCount { get; set; }
}
