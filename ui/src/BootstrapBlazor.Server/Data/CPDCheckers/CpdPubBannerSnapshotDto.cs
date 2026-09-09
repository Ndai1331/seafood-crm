using Newtonsoft.Json;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// DTO for CPD Pub Banner Snapshot from API
/// </summary>
public class CpdPubBannerSnapshotDto
{
    [JsonProperty("id")]
    public ulong Id { get; set; }
    
    [JsonProperty("cpd_checker_tool_row_id")]
    public ulong CpdCheckerToolRowId { get; set; }
    
    // Version 1 (mới nhất)
    [JsonProperty("banner_url_1")]
    public string? BannerUrl1 { get; set; }
    
    [JsonProperty("check_date_1")]
    public string? CheckDate1String { get; set; }
    
    public DateOnly? CheckDate1 
    { 
        get 
        {
            if (string.IsNullOrEmpty(CheckDate1String)) return null;
            return DateOnly.TryParse(CheckDate1String, out var date) ? date : null;
        }
    }
    
    [JsonProperty("brand_1")]
    public string? Brand1 { get; set; }
    
    [JsonProperty("utm_source_1")]
    public string? UtmSource1 { get; set; }
    
    [JsonProperty("affid_1")]
    public string? Affid1 { get; set; }
    
    // Version 2
    [JsonProperty("banner_url_2")]
    public string? BannerUrl2 { get; set; }
    
    [JsonProperty("check_date_2")]
    public string? CheckDate2String { get; set; }
    
    public DateOnly? CheckDate2 
    { 
        get 
        {
            if (string.IsNullOrEmpty(CheckDate2String)) return null;
            return DateOnly.TryParse(CheckDate2String, out var date) ? date : null;
        }
    }
    
    [JsonProperty("brand_2")]
    public string? Brand2 { get; set; }
    
    [JsonProperty("utm_source_2")]
    public string? UtmSource2 { get; set; }
    
    [JsonProperty("affid_2")]
    public string? Affid2 { get; set; }
    
    // Version 3
    [JsonProperty("banner_url_3")]
    public string? BannerUrl3 { get; set; }
    
    [JsonProperty("check_date_3")]
    public string? CheckDate3String { get; set; }
    
    public DateOnly? CheckDate3 
    { 
        get 
        {
            if (string.IsNullOrEmpty(CheckDate3String)) return null;
            return DateOnly.TryParse(CheckDate3String, out var date) ? date : null;
        }
    }
    
    [JsonProperty("brand_3")]
    public string? Brand3 { get; set; }
    
    [JsonProperty("utm_source_3")]
    public string? UtmSource3 { get; set; }
    
    [JsonProperty("affid_3")]
    public string? Affid3 { get; set; }
    
    // Version 4 (cũ nhất)
    [JsonProperty("banner_url_4")]
    public string? BannerUrl4 { get; set; }
    
    [JsonProperty("check_date_4")]
    public string? CheckDate4String { get; set; }
    
    public DateOnly? CheckDate4 
    { 
        get 
        {
            if (string.IsNullOrEmpty(CheckDate4String)) return null;
            return DateOnly.TryParse(CheckDate4String, out var date) ? date : null;
        }
    }
    
    [JsonProperty("brand_4")]
    public string? Brand4 { get; set; }
    
    [JsonProperty("utm_source_4")]
    public string? UtmSource4 { get; set; }
    
    [JsonProperty("affid_4")]
    public string? Affid4 { get; set; }
    
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

