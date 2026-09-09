namespace BootstrapBlazor.Server.Data.SeoCostData;

/// <summary>Filter sent to api/task9/seo-cost-data. Mirrors the API contract field for field.</summary>
public class SeoCostDataFilterDto
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public string? Sort { get; set; }
    public string? FilterText { get; set; }
    public DateTime? FromMonth { get; set; }
    public DateTime? ToMonth { get; set; }
    public string? Team { get; set; }
    public string? Pic { get; set; }
    public string? Domain { get; set; }
    public string? PaymentStatus { get; set; }
    public string? DeXuat { get; set; }
    public string? NhomKey { get; set; }
    public string? IdPhieu { get; set; }
}

/// <summary>One row of the SEO cost sheet as returned by the API.</summary>
public class SeoCostDataRowDto
{
    public long Id { get; set; }
    public long? Stt { get; set; }
    public string? Team { get; set; }
    public string? Thang { get; set; }
    public DateTime? ThangDate { get; set; }
    public string? DeXuat { get; set; }
    public string? NoiDungDeXuat { get; set; }
    public string? Domain { get; set; }
    public string? KeyMain { get; set; }
    public string? NhomKey { get; set; }
    public string? DoKho { get; set; }
    public long? Volume { get; set; }
    public string? TinhTrangSuDung { get; set; }
    public string? FileBaoGia { get; set; }
    public string? TeleDoiTac { get; set; }
    public string? FileOrder { get; set; }
    public double? SoLuong { get; set; }
    public decimal? SoTien { get; set; }
    public string? CkPct { get; set; }
    public long? ThanhToanVnd { get; set; }
    public double? ThanhToanUsdt { get; set; }
    public decimal? TongThanhToan { get; set; }
    public string? Pic { get; set; }
    public string? GiaiTrinh { get; set; }
    public string? TinhTrangThanhToan { get; set; }

    /// <summary>Brand arrives already masked from the API (****6701). Never un-mask it here.</summary>
    public string? Brand { get; set; }

    public string? NgayLenPhieu { get; set; }
    public string? IdPhieu { get; set; }
    public string? LeadSeo { get; set; }
    public string? IcCheck { get; set; }
    public string? FinalCheckAsst { get; set; }
    public string? DuyetHead { get; set; }
    public DateTime? SyncedAt { get; set; }
}

/// <summary>Values available in the dropdown filters.</summary>
public class SeoCostDataFilterOptionsDto
{
    public List<string> Teams { get; set; } = new();
    public List<string> Pics { get; set; } = new();
    public List<string> PaymentStatuses { get; set; } = new();
    public List<string> DeXuats { get; set; } = new();
    public List<string> NhomKeys { get; set; } = new();
}
