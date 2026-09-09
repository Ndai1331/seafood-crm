namespace BootstrapBlazor.Server.Data.SeoCostOverview;

public class SeoCostOverviewFilterDto
{
    public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
    public DateTime ToDate { get; set; } = DateTime.Today;
    public string? Pic { get; set; }
    /// <summary>
    /// A raw value of seo_cost_canonical.tinh_trang_su_dung ("Đang SEO", "Ngừng SEO", ...).
    /// The dropdown loads its options from that same column; sending a derived code such as
    /// "Active" matches nothing, because no row stores one.
    /// </summary>
    public string? Status { get; set; }
    public string? Keyword { get; set; }
    public List<string>? Keywords { get; set; }
    public string? NhomKey { get; set; }
    public string? Team { get; set; }
    public string? LoaiNhanSu { get; set; }
    public string? Level { get; set; }
    public string? Domain { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 20;
}

public class SeoCostOverviewKeywordDetailFilterDto
{
    public string Pic { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool IncludeSecondary { get; set; } = true;
}

public class SeoCostOverviewKeywordRankingDetailDto
{
    public string Domain { get; set; } = string.Empty;
    public int Rank { get; set; }
    public DateTime RankingDate { get; set; }
}

public class SeoCostOverviewKeywordDetailDto
{
    public List<SeoCostOverviewKeywordRankingDetailDto> PrimaryRankings { get; set; } = new();
    public List<SeoCostOverviewSecondaryKeywordDto> SecondaryKeywords { get; set; } = new();
}

public class SeoCostOverviewDashboardDto
{
    public SeoCostOverviewKpiDto Kpis { get; set; } = new();
    public List<SeoCostOverviewTrendPointDto> RankingTrend { get; set; } = new();
    public List<SeoCostOverviewDistributionItemDto> KeywordDistribution { get; set; } = new();
    public List<SeoCostOverviewCostTrendPointDto> CostTrend { get; set; } = new();
    public List<SeoCostOverviewKeywordRowDto> Keywords { get; set; } = new();
    public List<SeoCostOverviewTopDomainDetailDto> DomainsOnTopDetails { get; set; } = new();
    public List<SeoCostOverviewTopDomainDetailDto> DomainsOutTopDetails { get; set; } = new();
    public List<SeoCostOverviewTopHitDetailDto> HitsTop130Details { get; set; } = new();
    public int TotalKeywords { get; set; }
    public int TotalRows { get; set; }
    public int FilterPeriodDays { get; set; }
}

public class SeoCostOverviewSecondaryKeywordDto
{
    public string Keyword { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public int? CurrentRank { get; set; }
    public int? BestRank { get; set; }
    public int? Trend { get; set; }
    public DateTime? LastUpdate { get; set; }
}

public class SeoCostOverviewSecondaryKeywordFilterDto
{
    public string Pic { get; set; } = string.Empty;
    public string PrimaryKeyword { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class SeoCostOverviewTopDomainDetailDto
{
    public string Domain { get; set; } = string.Empty;
    public int BestRank { get; set; }
    public int DayCount { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public string PrimaryKeyword { get; set; } = string.Empty;
    public string Pic { get; set; } = string.Empty;
}

public class SeoCostOverviewTopHitDetailDto
{
    public DateTime Date { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public string PrimaryKeyword { get; set; } = string.Empty;
    public string Pic { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public int Rank { get; set; }
}

public class SeoCostOverviewKpiDto
{
    public KpiMetricDto TotalKeywords { get; set; } = new();
    public KpiMetricDto ActiveKeywords { get; set; } = new();
    public KpiMetricDto StoppedKeywords { get; set; } = new();
    public KpiMetricDto DomainsActive { get; set; } = new();
    public KpiMetricDto DomainsInactive { get; set; } = new();
    public KpiMetricDto DomainsOnTop { get; set; } = new();
    public KpiMetricDto DomainsOutTop { get; set; } = new();
    public KpiMetricDto HitsTop130 { get; set; } = new();
    public KpiMetricDto TotalCost { get; set; } = new();
}

public class KpiMetricDto
{
    public decimal Value { get; set; }
    public decimal? ChangePercent { get; set; }
}

public class SeoCostOverviewTrendPointDto
{
    public DateTime Date { get; set; }
    public int Top1To3 { get; set; }
    public int Top4To10 { get; set; }
    public int Top11To30 { get; set; }
    public int OutOfTop { get; set; }
    public List<string> Top1To3Domains { get; set; } = new();
    public List<string> Top4To10Domains { get; set; } = new();
    public List<string> Top11To30Domains { get; set; } = new();
    public List<string> OutOfTopDomains { get; set; } = new();
}

public class SeoCostOverviewDistributionItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public int TotalDays { get; set; }
    public decimal Percent { get; set; }
}

public class SeoCostOverviewSeoPeriodDto
{
    public DateTime FromMonth { get; set; }
    public DateTime ToMonth { get; set; }
    public string Label { get; set; } = string.Empty;
    public int? BestRank { get; set; }
    public decimal Cost { get; set; }
}

public class SeoCostOverviewCostTrendPointDto
{
    public DateTime Date { get; set; }
    public decimal TotalCost { get; set; }
    public int ActiveKeywordCount { get; set; }
}

public class SeoCostOverviewKeywordRowDto
{
    public string Keyword { get; set; } = string.Empty;
    public string? NhomKey { get; set; }
    public string Pic { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int? CurrentRank { get; set; }
    public int? BestRank { get; set; }
    public int? Trend { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime? LastUpdate { get; set; }
    public string? RankingDomain { get; set; }
    public List<SeoCostOverviewSeoPeriodDto> SeoPeriods { get; set; } = new();
    public List<SeoCostOverviewSecondaryKeywordDto> SecondaryKeywords { get; set; } = new();
    public int SecondaryKeywordCount { get; set; }
}

public class SeoCostOverviewKeywordSummaryRowDto
{
    public string Keyword { get; set; } = string.Empty;
    public int ActivePicCount { get; set; }
    public int StoppedPicCount { get; set; }
    public int ActiveDomainCount { get; set; }
    public int StoppedDomainCount { get; set; }
    public decimal TotalCost { get; set; }
}

public class SeoCostOverviewKeywordOverviewDto
{
    public List<SeoCostOverviewKeywordSummaryRowDto> Keywords { get; set; } = new();
    public int TotalCount { get; set; }
}

public class SeoCostOverviewFilterOptionsDto
{
    public List<string> Pics { get; set; } = new();
    public List<string> NhomKeys { get; set; } = new();
    public List<SeoCostOverviewStatusOptionDto> Statuses { get; set; } = new();
    public List<string> Teams { get; set; } = new();
    public List<string> LoaiNhanSus { get; set; } = new();
    public List<string> Levels { get; set; } = new();
    public List<string> Domains { get; set; } = new();
}

public class SeoCostOverviewStatusOptionDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class SeoKeywordPerformanceV2FilterDto
{
    public string? Month { get; set; }
    public DateTime FromDate { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime ToDate { get; set; } = DateTime.Today;
    public string? KeywordGroup { get; set; }
    public string? Keyword { get; set; }
    public string? Pic { get; set; }
    public string? Domain { get; set; }
    public string? Status { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 20;
}

public class SeoKeywordPerformanceV2DashboardDto
{
    public DateTime UpdatedAt { get; set; }
    public string Month { get; set; } = string.Empty;
    public decimal TotalDirectCost { get; set; }
    public List<SeoKeywordPerformanceV2KpiCardDto> Kpis { get; set; } = new();
    public List<SeoKeywordPerformanceV2ClassificationDto> KeywordClassification { get; set; } = new();
    public List<SeoKeywordPerformanceV2RowDto> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}

public class SeoKeywordPerformanceV2KpiCardDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string ValueText { get; set; } = string.Empty;
    public decimal? Change { get; set; }
    public bool LowerIsBetter { get; set; }
    public List<decimal> Sparkline { get; set; } = new();
}

public class SeoKeywordPerformanceV2ClassificationDto
{
    public string Status { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percent { get; set; }
}

public class SeoKeywordPerformanceV2RowDto
{
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string KeywordGroup { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public int? CurrentRank { get; set; }
    public int? RankChange { get; set; }
    public int? BestRank { get; set; }
    public int MonthlySearchVolume { get; set; }
    public List<string> Seos { get; set; } = new();
    public int SeoCount { get; set; }
    public decimal CurrentMonthCost { get; set; }
    public decimal AverageCostPerTop10 { get; set; }
    public List<SeoKeywordPerformanceV2RankHistoryDto> RankHistory { get; set; } = new();
    public string Note { get; set; } = string.Empty;
}

public class SeoKeywordPerformanceV2RankHistoryDto
{
    public string Month { get; set; } = string.Empty;
    public int? Rank { get; set; }
}

public class SeoKeywordPerformanceV2FilterOptionsDto
{
    public List<string> Months { get; set; } = new();
    public List<string> KeywordGroups { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public List<string> Pics { get; set; } = new();
    public List<string> Domains { get; set; } = new();
    public List<SeoCostOverviewStatusOptionDto> Statuses { get; set; } = new();
}
