using Newtonsoft.Json;

namespace BootstrapBlazor.Server.Data;
public class CpdCheckerToolRow
{
    [JsonProperty("id")]
    public ulong Id { get; set; }

    private string _originalLink = string.Empty;
    
    [JsonProperty("original_link")]
    public string OriginalLink 
    { 
        get => _originalLink;
        set => _originalLink = value ?? string.Empty;
    }
    
    // Fallback property for camelCase API response (API returns "originalLink" instead of "original_link")
    [JsonProperty("originalLink")]
    public string OriginalLinkCamelCase
    {
        get => _originalLink;
        set => _originalLink = value ?? _originalLink;
    }

    [JsonProperty("affid")]
    public string Affid { get; set; } = string.Empty;

    [JsonProperty("referrer_domain")]
    public string? ReferrerDomain { get; set; }
    // Fallback camelCase mapping from API responses
    [JsonProperty("referrerDomain")]
    public string? ReferrerDomainCamel
    {
        get => ReferrerDomain;
        set => ReferrerDomain = value ?? ReferrerDomain;
    }

    [JsonProperty("utm_source")]
    public string? UtmSource { get; set; }
    [JsonProperty("utmSource")]
    public string? UtmSourceCamel
    {
        get => UtmSource;
        set => UtmSource = value ?? UtmSource;
    }

    [JsonProperty("utm_medium")]
    public string? UtmMedium { get; set; }
    [JsonProperty("utmMedium")]
    public string? UtmMediumCamel
    {
        get => UtmMedium;
        set => UtmMedium = value ?? UtmMedium;
    }

    [JsonProperty("utm_content")]
    public string? UtmContent { get; set; }
    [JsonProperty("utmContent")]
    public string? UtmContentCamel
    {
        get => UtmContent;
        set => UtmContent = value ?? UtmContent;
    }

    [JsonProperty("utm_campaign")]
    public string? UtmCampaign { get; set; }
    [JsonProperty("utmCampaign")]
    public string? UtmCampaignCamel
    {
        get => UtmCampaign;
        set => UtmCampaign = value ?? UtmCampaign;
    }

    [JsonProperty("pic")]
    public string Pic { get; set; } = string.Empty;

    [JsonProperty("brand")]
    public string Brand { get; set; } = string.Empty;

    [JsonProperty("pub_link")]
    public string PubLink { get; set; } = string.Empty;

    private string _shortLink = string.Empty;
    
    [JsonProperty("short_link")]
    public string ShortLink 
    { 
        get => _shortLink;
        set => _shortLink = value ?? string.Empty;
    }
    
    // Fallback property for camelCase API response (API returns "shortLink" instead of "short_link")
    [JsonProperty("shortLink")]
    public string ShortLinkCamelCase
    {
        get => _shortLink;
        set => _shortLink = value ?? _shortLink;
    }

    [JsonProperty("result_shortlink")]
    public bool? ResultShortlink { get; set; }

    [JsonProperty("short_link_new")]
    public string? ShortLinkNew { get; set; }

    [JsonProperty("note")]
    public string? Note { get; set; }

    [JsonProperty("result_pub_image")]
    public string? ResultPubImage { get; set; }
    // Fallback camelCase mapping from API responses
    [JsonProperty("resultPubImage")]
    public string? ResultPubImageCamel
    {
        get => ResultPubImage;
        set => ResultPubImage = value ?? ResultPubImage;
    }

    [JsonProperty("qc_summarize")]
    public string? QcSummarize { get; set; }
    // Fallback camelCase mapping from API responses
    [JsonProperty("qcSummarize")]
    public string? QcSummarizeCamel
    {
        get => QcSummarize;
        set => QcSummarize = value ?? QcSummarize;
    }

    [JsonProperty("result_manual_by_qc")]
    public string? ResultManualByQc { get; set; }
    // Fallback camelCase mapping from API responses
    [JsonProperty("resultManualByQc")]
    public string? ResultManualByQcCamel
    {
        get => ResultManualByQc;
        set => ResultManualByQc = value ?? ResultManualByQc;
    }

    [JsonProperty("is_auto")]
    public int? IsAuto { get; set; }
    // Fallback camelCase mapping from API responses
    [JsonProperty("isAuto")]
    public int? IsAutoCamel
    {
        get => IsAuto;
        set => IsAuto = value ?? IsAuto;
    }

    [JsonProperty("is_live")]
    public int? IsLive { get; set; }
    [JsonProperty("isLive")]
    public int? IsLiveCamel { get => IsLive; set => IsLive = value ?? IsLive; }

    [JsonProperty("is_mobile")]
    public int? IsMobile { get; set; }
    [JsonProperty("isMobile")]
    public int? IsMobileCamel { get => IsMobile; set => IsMobile = value ?? IsMobile; }

    [JsonProperty("is_proxy")]
    public int? IsProxy { get; set; }
    [JsonProperty("isProxy")]
    public int? IsProxyCamel { get => IsProxy; set => IsProxy = value ?? IsProxy; }

    [JsonProperty("banner_type")]
    public string? BannerType { get; set; }
    [JsonProperty("bannerType")]
    public string? BannerTypeCamel { get => BannerType; set => BannerType = value ?? BannerType; }

    [JsonProperty("deleted")]
    public int? Deleted { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Backward compatibility properties
    [JsonProperty("no")]
    public string No { get; set; } = string.Empty;

    [JsonProperty("publink")]
    public string Publink
    {
        get => PubLink;
        set => PubLink = value;
    }

    // Legacy properties - removed from DB but kept for backward compatibility
    // Used by ScrapingbeeCheckerService, LinkCheckerService, CpdCheckerTool
    public string? RedirectShortLink { get; set; }
    public string? DeviceCheck { get; set; }
    public string? CheckLink { get; set; }
    public string? FileNameBanner { get; set; }
    public string? FolderBanner { get; set; }
    public string? Title { get; set; }
    public string? Alt { get; set; }
    public string? Dimension { get; set; }
    public string? ResultShortlinkStatus { get; set; }
    public string? ResultShortlinkString { get; set; }
    public string? ResultAlt { get; set; }
    public string? ResultAltStatus { get; set; }
    public string? ResultTitle { get; set; }
    public string? ResultTitleStatus { get; set; }
    public string? BannerCheckNote { get; set; }
    public string? BannerUrl { get; set; }
    public string? Position { get; set; }

    [JsonProperty("original_link_new")]
    public string? OriginalLinkNew { get; set; }

    [JsonProperty("originalLinkNew")]
    public string? OriginalLinkNewCamel
    {
        get => OriginalLinkNew;
        set => OriginalLinkNew = value ?? OriginalLinkNew;
    }
}
