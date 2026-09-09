using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data.CPDCheckers;

/// <summary>
/// Time-series banner snapshot DTO (v2). Flat structure, 1 row per scan per row_id.
/// Replaces the 4-slot CpdPubBannerSnapshotDto for new API endpoint.
/// </summary>
public class CpdPubBannerSnapshotV2Dto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("row_id")]
    public int RowId { get; set; }

    [JsonPropertyName("scan_id")]
    public string? ScanId { get; set; }

    [JsonPropertyName("scan_date")]
    public DateTime ScanDate { get; set; }

    [JsonPropertyName("banner_url")]
    public string? BannerUrl { get; set; }

    [JsonPropertyName("brand")]
    public string? Brand { get; set; }

    [JsonPropertyName("affid")]
    public string? Affid { get; set; }

    [JsonPropertyName("utm_source")]
    public string? UtmSource { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
