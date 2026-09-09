namespace BootstrapBlazor.Server.Data.DomainAges;

public sealed record DomainJourneyResponse
{
    public string Domain { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAtUtc { get; init; }

    public DomainJourneyPeriod Period { get; init; } = new();

    public string Status { get; init; } = "empty";

    public DomainJourneySummary Summary { get; init; } = new();

    public List<DomainJourneyMetricPoint> MetricHistory { get; init; } = [];

    public List<DomainJourneyKeywordPoint> KeywordHistory { get; init; } = [];

    public List<DomainJourneyRatingPoint> DomainRatingHistory { get; init; } = [];

    public List<DomainJourneyRefDomainPoint> RefDomainHistory { get; init; } = [];

    public List<DomainJourneyTopKeyword> TopKeywords { get; init; } = [];

    public List<DomainJourneyTopPage> TopPages { get; init; } = [];

    public List<DomainJourneyReferringDomain> ReferringDomains { get; init; } = [];

    public List<DomainJourneyPartialError> PartialErrors { get; init; } = [];
}

public sealed record DomainJourneyPeriod
{
    public string From { get; init; } = string.Empty;

    public string To { get; init; } = string.Empty;

    public string Grouping { get; init; } = "monthly";

    public int Months { get; init; } = 24;
}

public sealed record DomainJourneySummary
{
    public double? DomainRating { get; init; }

    public double? OrganicTraffic { get; init; }

    public double? OrganicKeywords { get; init; }

    public double? ReferringDomains { get; init; }
}

public sealed record DomainJourneyMetricPoint
{
    public string Date { get; init; } = string.Empty;

    public double? OrganicTraffic { get; init; }

    public double? OrganicKeywords { get; init; }
}

public sealed record DomainJourneyKeywordPoint
{
    public string Date { get; init; } = string.Empty;

    public double? Total { get; init; }

    public double? Top3 { get; init; }

    public double? Positions4To10 { get; init; }

    public double? Positions11To20 { get; init; }

    public double? Positions21To50 { get; init; }

    public double? Positions51Plus { get; init; }
}

public sealed record DomainJourneyRatingPoint
{
    public string Date { get; init; } = string.Empty;

    public double? DomainRating { get; init; }
}

public sealed record DomainJourneyRefDomainPoint
{
    public string Date { get; init; } = string.Empty;

    public double? ReferringDomains { get; init; }
}

public sealed record DomainJourneyTopKeyword
{
    public string Keyword { get; init; } = string.Empty;

    public double? BestPosition { get; init; }

    public double? Volume { get; init; }

    public double? Traffic { get; init; }

    public string? Url { get; init; }
}

public sealed record DomainJourneyTopPage
{
    public string Url { get; init; } = string.Empty;

    public double? Traffic { get; init; }

    public double? Value { get; init; }

    public string? TopKeyword { get; init; }

    public double? Keywords { get; init; }
}

public sealed record DomainJourneyReferringDomain
{
    public string Domain { get; init; } = string.Empty;

    public double? DomainRating { get; init; }

    public double? Backlinks { get; init; }

    public double? DofollowLinks { get; init; }

    public string? FirstSeen { get; init; }
}

public sealed record DomainJourneyPartialError
{
    public string Source { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}

public enum DomainInvestigationBand
{
    WorthConsidering,
    NeedsInvestigation,
    HighRisk,
}

public sealed record DomainInvestigationDecision
{
    public required DomainInvestigationBand Band { get; init; }

    public required int Score { get; init; }

    public required int AvailableSources { get; init; }

    public const int TotalSources = 7;

    public List<string> PositiveSignals { get; init; } = [];

    public List<string> RiskSignals { get; init; } = [];

    public List<string> DataGaps { get; init; } = [];
}

/// <summary>
/// Một điểm lịch sử theo tháng cho panel. Lấy thẳng từ endpoint gốc của Ahrefs (metrics-history,
/// domain-rating-history) thay vì qua domain-journey — journey trả partial cho nhiều domain
/// khiến panel báo "không có lịch sử" trong khi cột ngay cạnh vẫn có số.
/// </summary>
public sealed record MetricHistoryPoint(DateOnly? Month, double? Value);

/// <summary>
/// Mức gộp lịch sử, khớp đúng ba lựa chọn trên chart Ahrefs. Gộp theo tháng nén mất đỉnh của
/// domain chỉ sống vài ngày, nên đây là tham số người dùng phải tự chọn được.
/// </summary>
public enum TrafficGrouping
{
    Daily,
    Weekly,
    Monthly,
}

/// <summary>
/// Bộ lọc referring domains, khớp các filter trên báo cáo Referring domains của Ahrefs.
/// <paramref name="BestLinksOnly"/> bật preset "Best links: Only" của Ahrefs — bảy điều kiện
/// cùng lúc, không phải một cột dữ liệu.
/// </summary>
public sealed record RefDomainFilter(
    bool BestLinksOnly = false,
    bool DofollowOnly = false,
    bool ExcludeSpam = false);

/// <summary>Một dòng của báo cáo Referring domains.</summary>
public sealed record RefDomainRow(
    string? Domain,
    double? DomainRating,
    double? TrafficDomain,
    int? LinksToTarget,
    int? DofollowLinks,
    DateOnly? FirstSeen);
