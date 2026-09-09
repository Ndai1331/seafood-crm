namespace BootstrapBlazor.Server.Data.KeywordRanking;

/// <summary>
/// Thông tin một nhóm tháng trong trash — dùng cho trang quản lý trash
/// </summary>
public class KeywordRankingTrashGroupDto
{
    public string MonthLabel { get; set; } = string.Empty;
    public DateTime Month { get; set; }
    public int RecordCount { get; set; }
    public string DeleteReason { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
    public string DeletedByName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Request khôi phục data trash của một tháng
/// </summary>
public class RestoreMonthlyDataRequest
{
    public DateTime Month { get; set; }
}
