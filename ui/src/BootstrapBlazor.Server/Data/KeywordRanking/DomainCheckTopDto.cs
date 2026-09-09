namespace BootstrapBlazor.Server.Data.KeywordRanking;

/// <summary>
/// Bộ lọc gửi sang task9-api cho trang tra cứu check-top theo domain.
/// Giữ đúng tên field với Contract.KeywordRankings.DomainCheckTopFilter bên API.
/// </summary>
public class DomainCheckTopFilter
{
    /// <summary>Blob domain người dùng dán vào — API tự tách dòng/dấu phẩy và chuẩn hoá host.</summary>
    public string DomainsRaw { get; set; } = string.Empty;

    public List<string> Domains { get; set; } = new();

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    /// <summary>Bỏ qua khoảng ngày, quét toàn bộ lịch sử đang lưu.</summary>
    public bool AllTime { get; set; }

    public string Keyword { get; set; } = string.Empty;

    /// <summary>Hạng tối đa được tính là "top".</summary>
    public int TopThreshold { get; set; } = 10;

    /// <summary>Chỉ trả keyword từng lọt top trong kỳ.</summary>
    public bool OnlyOnTop { get; set; }
}

/// <summary>Một dòng kết quả: 1 domain × 1 keyword trong khoảng ngày đã chọn.</summary>
public class DomainCheckTopKeywordDto
{
    public string Domain { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
    public string PicName { get; set; } = string.Empty;
    public int Volume { get; set; }
    public int DaysTracked { get; set; }
    public int DaysOnTop { get; set; }
    public int DaysTop3 { get; set; }
    public int DaysTop1 { get; set; }
    public int BestRank { get; set; }
    public int WorstRank { get; set; }
    public double AverageRank { get; set; }
    public int LatestRank { get; set; }
    public DateTime? LatestDate { get; set; }
    public int FirstRank { get; set; }
    public DateTime? FirstDate { get; set; }

    /// <summary>Hạng đầu kỳ trừ hạng cuối kỳ: dương là thăng hạng, âm là tụt.</summary>
    public int RankDelta { get; set; }

    public double OnTopRatio { get; set; }
}

/// <summary>Tổng hợp mức domain, hiển thị thành thẻ tóm tắt phía trên bảng.</summary>
public class DomainCheckTopSummaryDto
{
    public string Domain { get; set; } = string.Empty;
    public int TotalKeywords { get; set; }
    public int KeywordsOnTop { get; set; }
    public int DaysOnTopTotal { get; set; }
    public int DaysTracked { get; set; }
    public int BestRank { get; set; }
    public double AverageRank { get; set; }
    public int TotalVolume { get; set; }
    public DateTime? LatestDate { get; set; }
}

public class DomainCheckTopResponseDto
{
    public List<DomainCheckTopSummaryDto> Summaries { get; set; } = new();
    public List<DomainCheckTopKeywordDto> Rows { get; set; } = new();
    public List<string> NotFoundDomains { get; set; } = new();

    /// <summary>Khoảng ngày đã áp dụng; ở chế độ toàn bộ lịch sử là khoảng thực tế có dữ liệu.</summary>
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int TopThreshold { get; set; }
    public bool AllTime { get; set; }
}
