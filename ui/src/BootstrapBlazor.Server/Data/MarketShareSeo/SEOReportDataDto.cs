using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Single row from report/seo-data API.
/// </summary>
public class SEOReportDataDto
{
    public string? Team { get; set; }
    // API mới (task9-api MarketShareSeo) serialize field này là "pic" — không map thì cột Nhân viên trống.
    // RequestClient deserialize bằng Newtonsoft (JsonConvert) nên PHẢI dùng JsonProperty;
    // giữ thêm JsonPropertyName cho chỗ nào parse bằng System.Text.Json.
    [Newtonsoft.Json.JsonProperty("pic")]
    [JsonPropertyName("pic")]
    public string? PicName { get; set; }
    public string? Keyword { get; set; }
    public string? Domain { get; set; }
    public string? Results { get; set; }
    public string? Date { get; set; }
    public string? Classification1 { get; set; }
}

/// <summary>
/// Paged response from report/seo-data API.
/// </summary>
public class SEOReportDataPagedResponseDto
{
    public List<SEOReportDataDto> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
