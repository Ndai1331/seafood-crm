namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Grouped publisher row - master row hiển thị tổng quan của 1 publisher
/// </summary>
public class GroupedCpdPublisher
{
    /// <summary>
    /// Pub Link - unique identifier của publisher
    /// </summary>
    public string PubLink { get; set; } = string.Empty;

    /// <summary>
    /// Brand name
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// PIC
    /// </summary>
    public string Pic { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số banners của publisher này
    /// </summary>
    public int TotalBanners { get; set; }

    /// <summary>
    /// Số banners có lỗi (bất kỳ field nào changed)
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Số banners có affid changed
    /// </summary>
    public int AffidChangedCount { get; set; }

    /// <summary>
    /// Số banners có domain changed
    /// </summary>
    public int DomainChangedCount { get; set; }

    /// <summary>
    /// Số banners có UTM medium changed
    /// </summary>
    public int UtmMediumChangedCount { get; set; }

    /// <summary>
    /// Số banners có short link changed
    /// </summary>
    public int ShortLinkChangedCount { get; set; }

    /// <summary>
    /// Số banners OK (status: PASS_ALL hoặc rỗng)
    /// </summary>
    public int OkCount { get; set; }

    /// <summary>
    /// Số banners bị remove (đã bị đánh dấu xoá hoặc scan không tìm thấy)
    /// </summary>
    public int RemovedCount { get; set; }

    /// <summary>
    /// Số banners mới phát hiện (is_auto = 1)
    /// </summary>
    public int NewBannerCount { get; set; }

    /// <summary>
    /// List các banners con (detail rows)
    /// </summary>
    public List<CpdCheckerToolRow> Children { get; set; } = new();

    /// <summary>
    /// Status summary: "3 lỗi, 12 OK" hoặc "Tất cả OK"
    /// </summary>
    public string StatusSummary { get; set; } = string.Empty;

    /// <summary>
    /// Ngày check gần nhất (max CreatedAt của children)
    /// </summary>
    public DateTime LastCheckedAt { get; set; }

    /// <summary>
    /// Banner mới nhất (ResultPubImage của banner check gần nhất)
    /// </summary>
    public string LatestBanner { get; set; } = string.Empty;

    /// <summary>
    /// Banner row mới nhất (toàn bộ object để hiển thị chi tiết)
    /// </summary>
    public CpdCheckerToolRow? LatestBannerRow { get; set; }
}
