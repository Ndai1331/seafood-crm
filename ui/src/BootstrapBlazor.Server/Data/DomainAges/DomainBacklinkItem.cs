namespace BootstrapBlazor.Server.Data.DomainAges;

/// <summary>
/// Một backlink trỏ về domain, lấy từ Ahrefs all-backlinks. Dùng riêng cho panel Backlinks —
/// domain-journey không trả dữ liệu ở mức backlink, chỉ có mức referring domain.
/// </summary>
public sealed record DomainBacklinkItem
{
    public string UrlFrom { get; init; } = string.Empty;

    /// <summary>DR của trang nguồn — tiêu chí chính để biết link mạnh hay rác.</summary>
    public double? DomainRatingSource { get; init; }

    public double? TrafficDomain { get; init; }

    public string? Anchor { get; init; }

    /// <summary>Ahrefs trả chuỗi ngày; giữ nguyên dạng chuỗi vì chỉ dùng để hiển thị.</summary>
    public string? FirstSeen { get; init; }

    /// <summary>Panel Bestlink: dòng là TRANG của domain, đây là số backlink trỏ về nó.</summary>
    public int? LinksToTarget { get; init; }
}

/// <summary>Loại số liệu mà panel đang mở — mỗi loại chỉ hiện đúng khối dữ liệu của nó.</summary>
public enum DomainMetricPanelKind
{
    DomainRating,
    RefDomains,
    Backlinks,
    Traffic,
    PeakTraffic,
    BestLink,
}
