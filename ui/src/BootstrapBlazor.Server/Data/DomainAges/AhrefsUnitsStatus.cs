namespace BootstrapBlazor.Server.Data.DomainAges;

/// <summary>
/// Units còn lại của account SEO dùng chung, đọc thẳng từ Ahrefs
/// (subscription-info-limits-and-usage) mỗi lần cần. Không lưu DB, không job theo dõi:
/// con số chỉ có nghĩa ngay lúc người dùng sắp bấm soi.
/// </summary>
public sealed record AhrefsUnitsStatus
{
    public long? Limit { get; init; }

    public long? Used { get; init; }

    /// <summary>Ngày quota reset — để người dùng biết chờ được hay phải mua thêm.</summary>
    public DateOnly? ResetDate { get; init; }

    public long? Remaining => Limit is null || Used is null
        ? null
        : Math.Max(0, Limit.Value - Used.Value);
}
