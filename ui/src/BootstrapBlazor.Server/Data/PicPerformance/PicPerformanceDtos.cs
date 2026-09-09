namespace BootstrapBlazor.Server.Data.PicPerformance;

// Client-side mirrors of Contract.Task9.PicPerformance DTOs

public class PicPerformanceFilterDto
{
    public const string UnassignedTeam = "__unassigned__";
    public string FromMonth { get; set; } = string.Empty; // "yyyy-MM", inclusive
    public string ToMonth { get; set; } = string.Empty;   // "yyyy-MM", inclusive
    public string? Team { get; set; }
    public string? Pic { get; set; }
    public string? StaffType { get; set; } // null | Inhouse | OS | OS Z
}

public class PicPerformanceTeamRowDto
{
    public string? Team { get; set; }
    public int PicCount { get; set; }
    /// <summary>Domain của team CÓ kết quả trong kỳ (unique toàn kỳ).</summary>
    public int DomainCount { get; set; }
    /// <summary>Mẫu số: tổng domain team đang phụ trách — ảnh chụp hiện tại.</summary>
    public int AssignedDomainCount { get; set; }
    public int Top1 { get; set; }
    public int Top3 { get; set; }
    public int Top10 { get; set; }
    public decimal Cost { get; set; }
    public decimal? CostPerTopHit { get; set; }
    public PicPerformanceCostBreakdownDto CostBreakdown { get; set; } = new();
}

public class PicPerformanceTeamsResponseDto
{
    public List<PicPerformanceTeamRowDto> Rows { get; set; } = new();
    public int RawTopHits { get; set; }
    public int PrevRawTopHits { get; set; }
    public int ExternalTopHits { get; set; }
    public decimal UnmatchedCost { get; set; }
    /// <summary>Bóc chi tiết khoản chưa khớp theo từng giá trị PIC ghi trên phiếu.</summary>
    public List<PicPerformanceUnmatchedCostRowDto> UnmatchedCostRows { get; set; } = new();
    public int ResultDomainCount { get; set; }
    /// <summary>Tổng domain (distinct) các PIC trong phạm vi đang được giao — mẫu số của "X / Y".</summary>
    public int AssignedDomainCount { get; set; }
    public List<PicPerformanceDailyPointDto> DailyTotals { get; set; } = new();
    public List<PicPerformanceMonthlyPointDto> Monthly { get; set; } = new();
    /// <summary>Tháng cuối đã có trong sổ chốt KPI (yyyy-MM) — biên trên của mọi số trên trang.</summary>
    public string? DataThroughMonth { get; set; }
    public PicPerformanceCostBreakdownDto CostBreakdown { get; set; } = new();
    public List<PicPerformanceKeywordCostRowDto> KeywordCosts { get; set; } = new();
    public int KeywordCostTotal { get; set; }
}

public class PicPerformancePicRowDto
{
    public string? Pic { get; set; }
    public string? StaffType { get; set; }
    public string? Level { get; set; }
    public int? TenureMonths { get; set; }
    public int AssignedDomainCount { get; set; }
    public int ResultDomainCount { get; set; }
    public int Top1 { get; set; }
    public int Top3 { get; set; }
    public int Top10 { get; set; }
    public int PrevTop10 { get; set; }
    public decimal Cost { get; set; }
    public decimal? CostPerTopHit { get; set; }
    public PicPerformanceCostBreakdownDto CostBreakdown { get; set; } = new();
}

public class PicPerformancePicsResponseDto
{
    public List<PicPerformancePicRowDto> Rows { get; set; } = new();
    public List<PicPerformanceDomainKeywordRowDto> DomainKeywords { get; set; } = new();
    public int DomainKeywordTotal { get; set; }
    public PicPerformancePicRowDto? Average { get; set; }
    /// <summary>Số domain (distinct) có kết quả của cả team — sum theo dòng PIC sẽ đếm trùng.</summary>
    public int ResultDomainCount { get; set; }
    /// <summary>Tổng domain (distinct) cả team đang được giao — mẫu số của "X / Y".</summary>
    public int AssignedDomainCount { get; set; }
    public List<PicPerformanceDailyPointDto> DailyTotals { get; set; } = new();
    public List<PicPerformanceMonthlyPointDto> Monthly { get; set; } = new();
    public PicPerformanceCostBreakdownDto CostBreakdown { get; set; } = new();
    public List<PicPerformanceKeywordCostRowDto> KeywordCosts { get; set; } = new();
    public int KeywordCostTotal { get; set; }
}

public class PicPerformanceDailyPointDto
{
    public string Date { get; set; } = string.Empty;
    public int Hits { get; set; }
}

public class PicPerformanceDomainRowDto
{
    public string Domain { get; set; } = string.Empty;
    public int Top1 { get; set; }
    public int Top3 { get; set; }
    public int Top10 { get; set; }
    public List<PicPerformanceDailyPointDto> Daily { get; set; } = new();
}

public class PicPerformanceDomainKeywordRowDto
{
    public string Domain { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
    public string? Pic { get; set; }
    public int Top1 { get; set; }
    public int Top3 { get; set; }
    public int Top10 { get; set; }
    public int SiteCountForKeyword { get; set; }
    /// <summary>Ngày gần nhất trong kỳ site này đạt Top 1-10.</summary>
    public string? LastTopDate { get; set; }
    /// <summary>Chi phí của SITE này trong kỳ (lặp trên mọi dòng keyword của cùng site).</summary>
    public decimal DomainCost { get; set; }
}

/// <summary>Yêu cầu popup chi tiết một keyword.</summary>
public class PicPerformanceKeywordDetailRequestDto : PicPerformanceFilterDto
{
    public string Keyword { get; set; } = string.Empty;
}

/// <summary>Toàn bộ nội dung popup keyword, gói trong MỘT lần gọi.</summary>
public class PicPerformanceKeywordDetailDto
{
    public string Keyword { get; set; } = string.Empty;
    public int PicCount { get; set; }
    public int DomainCount { get; set; }
    public int DomainWinCount { get; set; }
    public int TopHits { get; set; }
    public decimal Cost { get; set; }
    public List<PicPerformanceKeywordDomainRowDto> Rows { get; set; } = new();
    /// <summary>Chuỗi cho chart "theo PIC" của riêng key này, sắp theo lần đạt top giảm dần.</summary>
    public List<PicPerformanceKeywordPicRowDto> ByPic { get; set; } = new();
    /// <summary>Số site bị loại vì kho domain không đánh dấu Active.</summary>
    public int ExcludedInactiveCount { get; set; }
    /// <summary>Số site bị loại vì nằm ngoài Team/PIC đang lọc.</summary>
    public int ExcludedOutOfScopeCount { get; set; }
    /// <summary>Nhãn phạm vi đang áp dụng; null = toàn hệ thống.</summary>
    public string? ScopeLabel { get; set; }
}

/// <summary>Một PIC đang đẩy key này: bao nhiêu lần lên top, mấy site, tiêu bao nhiêu.</summary>
public class PicPerformanceKeywordPicRowDto
{
    public string Pic { get; set; } = string.Empty;
    public int TopHits { get; set; }
    public int DomainCount { get; set; }
    public decimal Cost { get; set; }
}

/// <summary>Một site đang chạy một keyword trong kỳ.</summary>
public class PicPerformanceKeywordDomainRowDto
{
    public string Domain { get; set; } = string.Empty;
    /// <summary>null = domain chưa có PIC trong sheet Phân loại → UI hiện "—".</summary>
    public string? Pic { get; set; }
    public string? Team { get; set; }
    public int Top1 { get; set; }
    public int Top3 { get; set; }
    public int Top10 { get; set; }
    /// <summary>null = site mới chỉ tiêu tiền cho key này, chưa lần nào lên top.</summary>
    public string? LastTopDate { get; set; }
    public decimal DomainCost { get; set; }
    /// <summary>Domain cuối chuỗi redirect; null = chưa kiểm được hoặc site không redirect đi đâu.</summary>
    public string? FinalDomain { get; set; }
    /// <summary>Tiền của các site KHÁC đã đẩy key này lên top cho site này. Không cộng vào tổng.</summary>
    public decimal SupportCost { get; set; }
    /// <summary>"redirect" = cùng domain cuối (chắc chắn) | "keyword" = chia theo lần đạt top (ước lượng).</summary>
    public string? SupportCostSource { get; set; }
    /// <summary>Từng ngày site này đạt top với key đó, mới nhất trước — cho dòng mở rộng.</summary>
    public List<PicPerformanceTopHistoryRowDto> History { get; set; } = new();
}

/// <summary>Yêu cầu popup "site này lên top key này những ngày nào".</summary>
public class PicPerformanceTopHistoryRequestDto : PicPerformanceFilterDto
{
    public string Domain { get; set; } = string.Empty;
    /// <summary>Bỏ trống = mọi keyword của site đó.</summary>
    public string Keyword { get; set; } = string.Empty;
}

/// <summary>Một ngày site đạt Top 1-10.</summary>
public class PicPerformanceTopHistoryRowDto
{
    public string Date { get; set; } = string.Empty;
    public int Rank { get; set; }
    public string? Keyword { get; set; }
}

public class PicPerformancePicOptionDto
{
    public string Pic { get; set; } = string.Empty;
    public string? Team { get; set; }
    public string? StaffType { get; set; }
}

public class PicPerformanceMonthlyPointDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal CostWin { get; set; }
    public int Top10 { get; set; }
}

public class PicPerformancePicDetailDto
{
    public string Pic { get; set; } = string.Empty;
    public string? Team { get; set; }
    public string? Level { get; set; }
    public string? StaffType { get; set; }
    public string? TinhTrang { get; set; }
    public int? TenureMonths { get; set; }
    public int Top1 { get; set; }
    public int Top3 { get; set; }
    public int Top10 { get; set; }
    public int PrevTop1 { get; set; }
    public int PrevTop3 { get; set; }
    public int PrevTop10 { get; set; }
    public decimal Cost { get; set; }
    public decimal PrevCost { get; set; }
    public decimal? CostPerTopHit { get; set; }
    public int ResultDomainCount { get; set; }
    public int AssignedDomainCount { get; set; }
    public double TeamAvgTop10 { get; set; }
    public decimal TeamAvgCost { get; set; }
    public List<PicPerformanceDailyPointDto> DailyTotals { get; set; } = new();
    public List<int> RankDistribution { get; set; } = new();
    public List<PicPerformanceMonthlyPointDto> Monthly { get; set; } = new();
    public List<PicPerformanceDomainKeywordRowDto> DomainKeywords { get; set; } = new();
    public int DomainKeywordTotal { get; set; }
    public PicPerformanceCostBreakdownDto CostBreakdown { get; set; } = new();
    public List<PicPerformanceKeywordCostRowDto> KeywordCosts { get; set; } = new();
    public int KeywordCostTotal { get; set; }
    /// <summary>% KPI của kỳ đang lọc + ma trận độ khó × dải Top cho popup.</summary>
    public PicPerformanceKpiDto Kpi { get; set; } = new();
    /// <summary>Phạm vi thực sự đang tính ("yyyy-MM").</summary>
    public string RangeFromMonth { get; set; } = string.Empty;
    public string RangeToMonth { get; set; } = string.Empty;
    /// <summary>true = xem toàn bộ lịch sử (chưa chọn tháng) → giấu mọi so sánh "kỳ trước".</summary>
    public bool IsFullHistory { get; set; }
}

/// <summary>
/// Chi phí tách "đạt / không đạt". CostAll và CostWin dùng CHUNG mẫu số TopHitCount — số lần đạt top.
/// </summary>
public class PicPerformanceCostBreakdownDto
{
    public decimal CostAll { get; set; }
    /// <summary>Chi phí phục vụ site có top (gồm domain trỏ link đúng key, KHÔNG gồm mua domain).</summary>
    public decimal CostWin { get; set; }
    /// <summary>Số domain có kết quả (unique toàn kỳ) — chỉ số hiển thị riêng, không phải mẫu số.</summary>
    public int ResultDomainCount { get; set; }
    /// <summary>Mẫu số dùng chung: số lần đạt top trong phạm vi.</summary>
    public int TopHitCount { get; set; }
    /// <summary>Chỉ số A = CostAll / TopHitCount. null → UI hiện "—".</summary>
    public decimal? CostPerTopHitAll { get; set; }
    /// <summary>Chỉ số B = CostWin / TopHitCount. null → UI hiện "—".</summary>
    public decimal? CostPerTopHitWin { get; set; }
    /// <summary>Hiệu quả chi phí = CostWin / CostAll, đơn vị %. null khi chưa tiêu đồng nào.</summary>
    public decimal? EfficiencyPct { get; set; }
    public Dictionary<string, decimal> CostByType { get; set; } = new();
    /// <summary>Tỷ giá USD→VNĐ đã dùng cho chi phí mua domain. 0 = không gồm chi phí mua domain.</summary>
    public decimal UsdToVndRate { get; set; }
}

/// <summary>Chỉ số C — chi phí gom theo từ khoá chia cho số lần chính key đó lên top.</summary>
public class PicPerformanceKeywordCostRowDto
{
    public string Keyword { get; set; } = string.Empty;
    /// <summary>
    /// Chi phí của key này trên TOÀN hệ thống. Tiền của mỗi domain được chia đều cho mọi key
    /// domain đó lên top, nên tổng cột này trên mọi key bằng đúng tổng chi phí — không rơi rụng.
    /// </summary>
    public decimal CostAll { get; set; }
    /// <summary>Lần đạt top của key này trên toàn hệ thống — mẫu số của chỉ số C.</summary>
    public int TopHits { get; set; }
    /// <summary>Trong đó, bao nhiêu lần là của phạm vi đang xem.</summary>
    public int ScopeTopHits { get; set; }
    /// <summary>null = key có tiền nhưng chưa lần nào lên top → UI hiện "—", KHÔNG hiện 0.</summary>
    public decimal? CostPerTop { get; set; }
    public int DomainCount { get; set; }
    public int DomainWinCount { get; set; }
    /// <summary>Số PIC đang đẩy key này: có site lên top với key, hoặc đang chi tiền cho key.</summary>
    public int PicCount { get; set; }
    public Dictionary<string, decimal> CostByType { get; set; } = new();
}

/// <summary>
/// Một giá trị PIC trên phiếu chi mà roster không có. Thực tế phần lớn là rác nguồn ("#REF!" do
/// công thức sheet gãy, "-"), nên phải xem được mặt chữ chứ không chỉ thấy một số tiền tổng.
/// </summary>
public class PicPerformanceUnmatchedCostRowDto
{
    /// <summary>Tên PIC y như trên phiếu.</summary>
    public string Pic { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public decimal Cost { get; set; }
    public string FirstMonth { get; set; } = string.Empty;
    public string LastMonth { get; set; } = string.Empty;
    /// <summary>Vài domain đại diện để nhận ra khoản chi này thuộc về việc gì.</summary>
    public string SampleDomains { get; set; } = string.Empty;
}

/// <summary>
/// % KPI của một PIC trong kỳ, tính bằng bảng trọng số của màn "Đánh giá hiệu suất SEO".
/// Xem bản gốc ở API: Contract/Task9/PicPerformance/PicPerformanceKpiDto.cs.
/// </summary>
public class PicPerformanceKpiDto
{
    /// <summary>null = chưa tính được — UI phải hiện lý do, không hiện 0%.</summary>
    public decimal? Percent { get; set; }
    public string? Classification { get; set; }
    public string? UnavailableReason { get; set; }
    public string? Level { get; set; }
    public int Months { get; set; }
    public decimal TotalPoints { get; set; }
    public int ScoredHits { get; set; }
    /// <summary>Lần đạt top chưa quy được độ khó — không tính điểm, phải hiện ra.</summary>
    public int UnscoredHits { get; set; }
    /// <summary>Ma trận độ khó × dải Top — chỉ gồm dải thực sự có lần đạt top.</summary>
    public List<PicPerformanceKpiRowDto> Rows { get; set; } = new();
    /// <summary>Từng cặp site × keyword đã sinh ra điểm — phần trả lời "tính từ cái gì ra".</summary>
    public List<PicPerformanceKpiItemDto> Items { get; set; } = new();
    /// <summary>Tổng số cặp; lớn hơn Items nghĩa là đã cắt bớt.</summary>
    public int ItemTotal { get; set; }
    /// <summary>Cùng phép tính tách theo tháng; rỗng khi kỳ chỉ có một tháng có dữ liệu.</summary>
    public List<PicPerformanceKpiMonthDto> ByMonth { get; set; } = new();
}

/// <summary>Một tháng của kỳ: % KPI, xếp loại và ma trận độ khó × dải Top của riêng tháng đó.</summary>
public class PicPerformanceKpiMonthDto
{
    /// <summary>Khoá tháng "yyyy-MM".</summary>
    public string Month { get; set; } = string.Empty;
    public decimal? Percent { get; set; }
    public string? Classification { get; set; }
    /// <summary>Mẫu số đã là 1 tháng nên bằng luôn % của tháng.</summary>
    public decimal TotalPoints { get; set; }
    public int ScoredHits { get; set; }
    /// <summary>Lần đạt top chưa quy được độ khó trong tháng — không tính điểm.</summary>
    public int UnscoredHits { get; set; }
    /// <summary>Chỉ gồm dải độ khó thực sự có lần đạt top trong tháng.</summary>
    public List<PicPerformanceKpiRowDto> Rows { get; set; } = new();
}

/// <summary>Một cặp site × keyword: số lần đạt top ở từng dải Top và điểm ra được.</summary>
public class PicPerformanceKpiItemDto
{
    public string Domain { get; set; } = string.Empty;
    public string? Keyword { get; set; }
    /// <summary>0 = không tra được search volume nên dòng này không tính điểm.</summary>
    public long Volume { get; set; }
    /// <summary>null = chưa quy được độ khó.</summary>
    public string? Difficulty { get; set; }
    public List<int> Counts { get; set; } = new();
    public List<decimal> Weights { get; set; } = new();
    public int TopHits { get; set; }
    /// <summary>Điểm của dòng, ĐÃ chia số tháng.</summary>
    public decimal Points { get; set; }
}

public class PicPerformanceKpiRowDto
{
    public string Difficulty { get; set; } = string.Empty;
    /// <summary>Đếm theo thứ tự Top 1 / Top 2-3 / Top 4-5 / Top 6-7 / Top 8-10.</summary>
    public List<int> Counts { get; set; } = new();
    public List<decimal> Weights { get; set; } = new();
    /// <summary>Điểm cả dòng, ĐÃ chia số tháng.</summary>
    public decimal Points { get; set; }
}
