using BootstrapBlazor.Server.Data.SeoRequests;

namespace BootstrapBlazor.Server.Data.SeoDomainInventories;

public class SeoDomainInventoryDto
{
    public long Id { get; set; }
    public long? SeoRequestId { get; set; }
    public int? PicId { get; set; }
    public string? PicName { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public DateTime? RequestTime { get; set; }
    public string? Domain { get; set; }
    public long? KeywordId { get; set; }
    public string? KeywordText { get; set; }
    public decimal? Price { get; set; }
    public string? DomainClassification { get; set; }
    public int? BrandId { get; set; }
    public string? NhomKey { get; set; }
    public string? Market { get; set; }
    public int? DifficultyId { get; set; }
    public string? DifficultyText { get; set; }
    public int? Volume { get; set; }
    public string? UsagePurpose { get; set; }
    public string StatusDomain { get; set; } = "Active";
    public string StatusKey { get; set; } = "SEO";
    public string? Note { get; set; }
    public string? QcCheck { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool CanEdit { get; set; }
    public int CascadeUpdatedCount { get; set; }
    public int InventoryUpdatedCount { get; set; }
    public bool NeedsBulkConfirm { get; set; }
    public int BulkCandidateCount { get; set; }
    public List<string> SampleDomains { get; set; } = new();
}

public class SeoDomainInventoryFilterDto : BaseFilterPagingDto
{
    public string? Domain { get; set; }
    public int? PicId { get; set; }
    public int? TeamId { get; set; }
    public string? StatusDomain { get; set; }
    public string? StatusKey { get; set; }
    public string? Keyword { get; set; }
}

public class PatchSeoDomainInventoryDto
{
    public long Id { get; set; }
    public string? StatusDomain { get; set; }
    public string? StatusKey { get; set; }
    public string? Note { get; set; }
    public string? QcCheck { get; set; }
    public bool ClearNote { get; set; }
    public bool ClearQcCheck { get; set; }
    public long? KeywordId { get; set; }
    public bool ClearKeyword { get; set; }
    public int? BrandId { get; set; }
    public bool ClearBrand { get; set; }
    public string? UsagePurpose { get; set; }
    public bool ClearUsagePurpose { get; set; }
    public bool ConfirmBulkStatusKey { get; set; }
}

public class PreviewStatusKeyBulkDto
{
    public long Id { get; set; }
    public string StatusKey { get; set; } = "";
}

public class PreviewStatusKeyBulkResultDto
{
    public int CandidateCount { get; set; }
    public bool RequiresConfirm { get; set; }
    public List<string> SampleDomains { get; set; } = new();
}

public static class SeoDomainInventoryStatusOptions
{
    /// <summary>STATUS (DOMAIN) pills — match Google Sheet inventory tab.</summary>
    public static readonly List<ColoredSelectOption> DomainOptions =
    [
        new() { Value = "Active", Text = "Active", BgColor = "#2E7D32", TextColor = "#FFFFFF" },
        new() { Value = "Inactive", Text = "Inactive", BgColor = "#8B0000", TextColor = "#FFFFFF" },
        new() { Value = "Bàn giao", Text = "Bàn giao", BgColor = "#FCE7D6", TextColor = "#7A3E12" },
        new() { Value = "No data", Text = "No data", BgColor = "#D8F3DC", TextColor = "#1B5E3B" },
        new() { Value = "Lỗi domain", Text = "Lỗi domain", BgColor = "#D9E2EC", TextColor = "#334E68" },
        new() { Value = "KGH", Text = "KGH", BgColor = "#EDE7F6", TextColor = "#4527A0" },
        new() { Value = "Khác", Text = "Khác", BgColor = "#343A40", TextColor = "#FFFFFF" },
    ];

    /// <summary>STATUS (KEY) — Ngừng SEO = red text only (sheet style).</summary>
    public static readonly List<ColoredSelectOption> KeyOptions =
    [
        new() { Value = "SEO", Text = "Đang SEO", BgColor = "#E8F5E9", TextColor = "#2E7D32" },
        new() { Value = "DUY_TRI_TOP", Text = "Duy trì TOP", BgColor = "#E3F2FD", TextColor = "#1565C0" },
        new() { Value = "NGUNG_SEO", Text = "Ngừng SEO", BgColor = "transparent", TextColor = "#C62828" },
        new() { Value = "KHAC", Text = "Khác", BgColor = "#F5F5F5", TextColor = "#212121" },
    ];
}
