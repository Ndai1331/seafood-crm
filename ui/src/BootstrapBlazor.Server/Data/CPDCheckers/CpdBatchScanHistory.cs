using Newtonsoft.Json;

namespace BootstrapBlazor.Server.Data;

public class CpdBatchScanHistory
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("scanId")]
    public string ScanId { get; set; } = string.Empty;

    [JsonProperty("scanDate")]
    public DateTime ScanDate { get; set; }

    [JsonProperty("rowId")]
    public int RowId { get; set; }

    [JsonProperty("pubLink")]
    public string PubLink { get; set; } = string.Empty;

    [JsonProperty("shortLink")]
    public string ShortLink { get; set; } = string.Empty;

    [JsonProperty("originalLink")]
    public string OriginalLink { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("affid")]
    public string? Affid { get; set; }

    [JsonProperty("domain")]
    public string? Domain { get; set; }

    [JsonProperty("finalUrl")]
    public string? FinalUrl { get; set; }

    [JsonProperty("newShortlink")]
    public string? NewShortlink { get; set; }

    [JsonProperty("foundHref")]
    public string? FoundHref { get; set; }

    [JsonProperty("foundImgSrc")]
    public string? FoundImgSrc { get; set; }

    [JsonProperty("bannerType")]
    public string BannerType { get; set; } = "image";

    [JsonProperty("statusChanged")]
    public bool StatusChanged { get; set; }

    [JsonProperty("previousStatus")]
    public string? PreviousStatus { get; set; }

    [JsonProperty("viewport")]
    public string Viewport { get; set; } = "desktop";

    [JsonProperty("isProxy")]
    public bool IsProxy { get; set; }

    [JsonProperty("isAuto")]
    public bool IsAuto { get; set; }

    [JsonProperty("screenshotUrl")]
    public string? ScreenshotUrl { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }
}
