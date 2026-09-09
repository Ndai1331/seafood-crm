namespace BootstrapBlazor.Server.Data.DomainAges;

/// <summary>
/// Kết quả một lượt hỏi Ahrefs. Phân biệt "chưa hỏi" với "hỏi rồi mà hỏng" là bắt buộc:
/// gộp hai thứ đó vào cùng một ô trống khiến bảng trông như đang khẳng định "không có dữ liệu"
/// trong khi thực ra chỉ là lỗi mạng một nhịp.
/// </summary>
public enum DomainMetricFetchState
{
    /// <summary>Chưa gọi lần nào — ô để trống mờ, không kết luận gì.</summary>
    NotFetched,

    /// <summary>Gọi được. Giá trị đi kèm là câu trả lời thật của Ahrefs, kể cả khi là 0 hoặc rỗng.</summary>
    Ok,

    /// <summary>Gọi hỏng (mạng/Ahrefs lỗi). Phải nói rõ và cho thử lại, KHÔNG được hiện như "không có".</summary>
    Failed,
}

/// <summary>
/// Số liệu giá trị SEO của một domain, lấp dần theo 2 tầng chi phí:
/// tầng rẻ (batch-analysis, 1 lượt gọi cho cả danh sách) và tầng sâu
/// (metrics-history + all-backlinks, 2 lượt gọi mỗi domain, chỉ chạy khi người dùng yêu cầu).
/// null = chưa lấy; 0 = Ahrefs trả về 0 thật.
/// </summary>
public sealed record DomainAgeMetrics
{
    // ── Tầng 2: batch-analysis ────────────────────────────────
    /// <summary>UR — URL Rating của homepage. Ahrefs không có lịch sử UR nên cột này không mở panel.</summary>
    public double? UrlRating { get; init; }

    public double? DomainRating { get; init; }

    /// <summary>AR — Ahrefs Rank. Cũng không có lịch sử, chỉ dùng để lọc trên bảng.</summary>
    public double? AhrefsRank { get; init; }

    public double? RefDomains { get; init; }

    /// <summary>Ref domains dofollow — Ahrefs UI gọi là "Followed".</summary>
    public double? RefDomainsFollowed { get; init; }

    public double? Backlinks { get; init; }

    /// <summary>Backlinks dofollow — Ahrefs UI gọi là "Followed".</summary>
    public double? BacklinksFollowed { get; init; }

    public double? OrgTraffic { get; init; }

    /// <summary>Ahrefs không có dữ liệu cho domain này (trả -1) — khác với "chưa lấy".</summary>
    public bool NoAhrefsData { get; init; }

    // ── Tầng 3: deep enrich ───────────────────────────────────
    /// <summary>Đỉnh organic traffic trong cửa sổ lịch sử đang xem (mặc định 5 năm).</summary>
    public double? PeakTraffic { get; init; }

    public DateOnly? PeakMonth { get; init; }

    /// <summary>
    /// Trạng thái lần hỏi lịch sử gần nhất. Ok + PeakTraffic null = Ahrefs thật sự không có
    /// lịch sử; Failed = gọi hỏng, phải cho thử lại.
    /// </summary>
    public DomainMetricFetchState PeakState { get; init; }

    /// <summary>Phạm vi quốc gia của số đỉnh đang giữ; null = toàn cầu.</summary>
    public string? PeakCountry { get; init; }

    public string? BestLinkUrl { get; init; }

    public double? BestLinkDr { get; init; }

    /// <summary>Số tên miền khác nhau đang còn trỏ về, đạt ngưỡng DR và là dofollow.</summary>
    public int? BestLinkCount { get; init; }

    /// <summary>Số đã chạm trần đếm — hiện "N+" thay vì con số tuyệt đối.</summary>
    public bool BestLinkCountCapped { get; init; }

    public DomainMetricFetchState BestLinkState { get; init; }

    public bool HasDeepData => PeakTraffic is not null || BestLinkUrl is not null || BestLinkCount is not null;
}
