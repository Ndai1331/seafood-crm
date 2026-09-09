namespace BootstrapBlazor.Server.Data.DomainRequestTracker;

public class DomainRequestTrackerFilterDto
{
    public string? Team { get; set; }
    public string? Pic { get; set; }
    /// <summary>Done / No / Cancel / "Đã bị mua" / "KHÔNG MUA ĐƯỢC" or "__PENDING__" for IS NULL</summary>
    public string? TrangThai { get; set; }
    /// <summary>Aged or New</summary>
    public string? PhanLoai { get; set; }
    /// <summary>DUYỆT or KHÔNG DUYỆT</summary>
    public string? DuyetMuaHead { get; set; }
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
}

public class DomainRequestTrackerItemDto
{
    public long Id { get; set; }
    public string? Team { get; set; }
    public string? Pic { get; set; }
    public string? Domain { get; set; }
    public string? KeyMain { get; set; }
    public decimal? Gia { get; set; }
    public string? PhanLoai { get; set; }
    public string? TrangThaiSauMua { get; set; }
    public string? DuyetMuaHead { get; set; }
    public DateTime? NgayDeXuat { get; set; }
}

public class DomainRequestTrackerSummaryDto
{
    public int TotalCount { get; set; }
    public int DoneCount { get; set; }
    public int PendingCount { get; set; }
    public decimal TotalDoneGia { get; set; }
}

public class DomainRequestTrackerResponseDto
{
    public List<DomainRequestTrackerItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public DomainRequestTrackerSummaryDto Summary { get; set; } = new();
}

public class DomainRequestTrackerFilterOptionsDto
{
    public List<string> Teams { get; set; } = new();
    public List<string> Pics { get; set; } = new();
    public List<string> TrangThaiList { get; set; } = new();
}
