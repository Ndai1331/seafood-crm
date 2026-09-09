namespace BootstrapBlazor.Server.Data.KeywordRanking;

/// <summary>
/// Request xóa data SEO theo tháng — chỉ dành cho ADMIN
/// </summary>
public class DeleteMonthlyDataRequest
{
    /// <summary>Tháng cần xóa</summary>
    public DateTime Month { get; set; }

    /// <summary>Lý do xóa (bắt buộc)</summary>
    public string Reason { get; set; } = string.Empty;
}
