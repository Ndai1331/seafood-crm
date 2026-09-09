using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Một PIC trong khối MKT02 kèm team theo roster (seo_pic_map_canonical).
///
/// Nguồn: sổ chốt KPI seo_data.seo_report_data — cùng nguồn với trang /pic-performance.
/// Tab "Nội bộ MKT02" cộng các dòng này theo team; tab "Chi tiết MKT02" giữ nguyên mức PIC,
/// nên tổng hai tab không thể lệch nhau.
/// </summary>
public class MarketShareInternalPicRowDto
{
    /// <summary>Tên PIC theo roster; null = chưa gán được PIC.</summary>
    [Newtonsoft.Json.JsonProperty("pic")]
    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    /// <summary>Nhóm hiển thị: SEO1…SEO6, OSNA, OSZ (gộp OSZ - SEO1 + OSZ - SEO2), "OS (chưa chia)" = phần ô gộp K Na chưa chia được, null = chưa gán team.</summary>
    [Newtonsoft.Json.JsonProperty("team")]
    [JsonPropertyName("team")]
    public string? Team { get; set; }

    /// <summary>Số lần đạt top trong kỳ.</summary>
    [Newtonsoft.Json.JsonProperty("totalTop10")]
    [JsonPropertyName("totalTop10")]
    public int TotalTop10 { get; set; }

    /// <summary>Trong đó bao nhiêu lần ở Top 1.</summary>
    [Newtonsoft.Json.JsonProperty("top1")]
    [JsonPropertyName("top1")]
    public int Top1 { get; set; }

    /// <summary>Trong đó bao nhiêu lần ở Top 1-3.</summary>
    [Newtonsoft.Json.JsonProperty("top3")]
    [JsonPropertyName("top3")]
    public int Top3 { get; set; }
}

/// <summary>Phân rã % thị phần nội bộ MKT02 theo PIC.</summary>
public class MarketShareInternalBreakdownDto
{
    [Newtonsoft.Json.JsonProperty("rows")]
    [JsonPropertyName("rows")]
    public List<MarketShareInternalPicRowDto> Rows { get; set; } = new();

    /// <summary>Tháng cuối đã có trong sổ chốt KPI (yyyy-MM) — biên trên của mọi số trả về.</summary>
    [Newtonsoft.Json.JsonProperty("dataThroughMonth")]
    [JsonPropertyName("dataThroughMonth")]
    public string? DataThroughMonth { get; set; }
}
