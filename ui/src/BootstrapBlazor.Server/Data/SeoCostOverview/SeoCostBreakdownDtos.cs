namespace BootstrapBlazor.Server.Data.SeoCostOverview;

/// <summary>
/// Filter for the cost-breakdown endpoint. Separate from <see cref="SeoCostOverviewFilterDto"/>
/// so the paging fields cannot leak in — the breakdown always returns every row, because
/// share-of-total and rank cannot be computed from a page.
/// </summary>
public class SeoCostBreakdownFilterDto
{
    public DateTime FromDate { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime ToDate { get; set; } = DateTime.Today;
    public string? Team { get; set; }
    public string? LoaiNhanSu { get; set; }
    public string? Level { get; set; }
    public string? Pic { get; set; }
    public string? Domain { get; set; }
    public string? NhomKey { get; set; }
    public string? Status { get; set; }
    public List<string>? Keywords { get; set; }
}

public class SeoCostBreakdownRowDto
{
    public string Name { get; set; } = string.Empty;
    public string? Team { get; set; }
    public bool IsUnassigned { get; set; }

    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public decimal Total { get; set; }

    public decimal PreviousTotal { get; set; }
    public decimal? ChangePercent { get; set; }
    public decimal SharePercent { get; set; }
    public int Rank { get; set; }

    public int KeywordCount { get; set; }
    public int DomainCount { get; set; }
    public decimal CostPerKeyword { get; set; }
    public decimal CostPerDomain { get; set; }
}

public class SeoCostKeyGroupRowDto : SeoCostBreakdownRowDto
{
    public decimal CostTop1To3 { get; set; }
    public decimal CostTop4To10 { get; set; }
    public decimal CostOutOfTop { get; set; }
    public decimal CostUnranked { get; set; }

    public int KeywordTop1To3 { get; set; }
    public int KeywordTop4To10 { get; set; }
    public int KeywordOutOfTop { get; set; }
    public int KeywordUnranked { get; set; }
}

public class SeoCostBreakdownSummaryDto
{
    /// <summary>Đã chi tiền cho đối tác.</summary>
    public decimal Paid { get; set; }

    /// <summary>IC đã nghiệm thu, chưa ra tiền: "Chờ duyệt thanh toán" + "Chưa thanh toán".</summary>
    public decimal Unpaid { get; set; }

    /// <summary>Chi phí của kỳ. Bằng đúng Paid + Unpaid.</summary>
    public decimal Total { get; set; }

    // ---- tiền chưa thành chi phí, để riêng để không thổi phồng tổng ----

    /// <summary>Đã duyệt, đối tác đang làm, chưa nghiệm thu. Là cam kết chi.</summary>
    public decimal Committed { get; set; }

    /// <summary>Mới xin, chưa duyệt.</summary>
    public decimal Requested { get; set; }

    /// <summary>Xin rồi bỏ, không phát sinh gì.</summary>
    public decimal Cancelled { get; set; }

    /// <summary>Trạng thái lạ ngoài 6 giá trị quy trình ghi. Bình thường bằng 0.</summary>
    public decimal Other { get; set; }

    public decimal PreviousTotal { get; set; }
    public decimal? ChangePercent { get; set; }

    public int PicCount { get; set; }
    public int KeywordCount { get; set; }
    public int DomainCount { get; set; }
    public decimal CostPerKeyword { get; set; }
    public int PreviousPicCount { get; set; }

    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime PreviousFromDate { get; set; }
    public DateTime PreviousToDate { get; set; }
}

public class SeoCostMonthPointDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public decimal Total { get; set; }
}

public class SeoCostBreakdownDto
{
    public SeoCostBreakdownSummaryDto Summary { get; set; } = new();
    public List<SeoCostBreakdownRowDto> Teams { get; set; } = new();
    public List<SeoCostBreakdownRowDto> Pics { get; set; } = new();

    /// <summary>Chi phí theo loại đề xuất (de_xuat): Textlink, Backlink, Entity, ...</summary>
    public List<SeoCostBreakdownRowDto> Types { get; set; } = new();
    public List<SeoCostKeyGroupRowDto> KeyGroups { get; set; } = new();
    public List<SeoCostMonthPointDto> MonthlyTrend { get; set; } = new();
}
