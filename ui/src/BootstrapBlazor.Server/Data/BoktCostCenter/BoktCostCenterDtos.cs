namespace BootstrapBlazor.Server.Data.BoktCostCenter;

/// <summary>Trang dữ liệu kèm tổng, trả trong một lượt gọi — xem endpoint `bokt-cost-center/page`.</summary>
public class BoktCostCenterPageDto
{
    public List<BoktCostCenterListItemDto> Items { get; set; } = [];
    public int Total { get; set; }
    public BoktCostCenterSummaryDto Summary { get; set; } = new();
}

public class BoktCostCenterFilterDto
{
    public DateTime FromDate { get; set; } = DateTime.Today.AddMonths(-3);
    public DateTime ToDate { get; set; } = DateTime.Today;
    public string? FilterText { get; set; }
    public int? Month { get; set; }
    public string? Supplier { get; set; }
    public string? Pic { get; set; }
    public string? Domain { get; set; }
    public string? CostType { get; set; }

    /// <summary>Chỉ phiếu đã xác nhận mới mang team — dòng lịch sử của sheet không khớp bộ lọc này.</summary>
    public string? Team { get; set; }

    /// <summary>Khớp theo đoạn chứa: gõ `5051` hay `BC125051` đều ra. Dấu `*` đầu bị bỏ.</summary>
    public string? Brand { get; set; }

    public string? IdentityType { get; set; }
    public string? IdentityValue { get; set; }

    /// <summary>Chỉ phiếu đang nằm trong con số "cần AI sửa": thiếu đơn vị tiền hoặc thiếu loại chi phí.</summary>
    public bool? OnlyNeedsAiFix { get; set; }

    /// <summary>Chỉ phiếu không ghi brand nào — nhóm bị loại khỏi ô "Tổng theo brand".</summary>
    public bool? OnlyNoBrand { get; set; }

    public string? Sort { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 20;
}

/// <summary>
/// Sửa phiếu tại trang Đối soát. Chỉ những trường CHẶN việc đối soát; số tiền và nội dung
/// đến từ ERP nên không sửa ở đây.
/// </summary>
public class UpdateBoktTicketDto
{
    public long Id { get; set; }
    public string? Pic { get; set; }
    public string? Team { get; set; }
    public string? OsSeo { get; set; }
    public string? LoaiChiPhi { get; set; }
    public string? AmountCurrency { get; set; }
}

public class UpdateBoktTicketResultDto
{
    public string Outcome { get; set; } = string.Empty;
    public long? Id { get; set; }
    public string IdPhieu { get; set; } = string.Empty;
    public string? Pic { get; set; }
    public string? PicSource { get; set; }
    public decimal? AmountValue { get; set; }
    public string? AmountCurrency { get; set; }
    public decimal? FxRateApplied { get; set; }
}

/// <summary>Tên nguồn của một dòng đối soát — phải khớp hằng phía API.</summary>
public static class BoktTicketSourceNames
{
    public const string Sheet = "sheet";
    public const string SupportAsst = "support-asst";
}

public class BoktCostCenterListItemDto
{
    public long RawId { get; set; }

    /// <summary>
    /// Xem <see cref="BoktTicketSourceNames"/>. Chỉ dòng "support-asst" sửa được — dòng sheet
    /// là dữ liệu quá khứ, không ghi ngược.
    /// </summary>
    public string Source { get; set; } = BoktTicketSourceNames.Sheet;

    public string? Team { get; set; }
    public string? OsSeo { get; set; }
    public DateTime? NgayKt { get; set; }
    public int? Month { get; set; }
    public string? TicketId { get; set; }

    /// <summary>Cột STT của sheet — thứ duy nhất phân biệt các cặp dòng trùng nhau mọi cột khác.</summary>
    public string? Stt { get; set; }

    public string? Creator { get; set; }
    public string? Supplier { get; set; }
    public string? AccountInfo { get; set; }
    public string? TicketContent { get; set; }
    public string? SheetName { get; set; }
    public int SourceRow { get; set; }

    /// <summary>Link mở sheet đối soát tại đúng dòng của phiếu; null với phiếu chưa vào sheet.</summary>
    public string? SheetRowUrl { get; set; }

    public string? Pic { get; set; }

    /// <summary>
    /// PIC không do người nhập mà do hệ thống suy từ domain của phiếu (pic_col bỏ trống).
    /// Màn hình phải cho thấy điều đó — số tiền của phiếu sẽ được tính cho người này.
    /// </summary>
    public bool PicInferred { get; set; }
    public string? CostType { get; set; }
    public decimal AmountVnd { get; set; }
    public decimal AmountUsdt { get; set; }
    public decimal AmountUsd { get; set; }
    public decimal AmountUnknown { get; set; }
    public string? AmountRaw { get; set; }
    public string? AmountCurrency { get; set; }

    /// <summary>Tổng phiếu quy về VND — thay cột S của sheet. Null khi chưa xác định đơn vị.</summary>
    public decimal? AmountVndConverted { get; set; }

    /// <summary>Cùng số tiền đó quy về USD. USDT tính ngang USD (1:1).</summary>
    public decimal? AmountUsdConverted { get; set; }

    /// <summary>Tiền mỗi brand khi chia đều — thay cột T của sheet. Null khi phiếu không có brand.</summary>
    public decimal? AmountVndPerBrand { get; set; }

    public string? Domains { get; set; }
    public int DomainCount { get; set; }
    public string? Brands { get; set; }
    public int BrandCount { get; set; }
    public string ParseStatus { get; set; } = string.Empty;
    public decimal ParseConfidence { get; set; }
    public bool IsPartial { get; set; }
    public bool NeedsCurrency { get; set; }
    public bool NeedsCostType { get; set; }
    public string? ReportUrl { get; set; }
    public string? RequestUrl { get; set; }
}

public class BoktCostCenterSummaryDto
{
    public int TotalTickets { get; set; }
    public int PartialTickets { get; set; }
    public int NeedsCurrencyTickets { get; set; }
    public int NeedsCostTypeTickets { get; set; }
    public int NeedsAiFixTickets { get; set; }
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public decimal TotalUsd { get; set; }
    public decimal TotalUnknown { get; set; }

    /// <summary>Tổng đã quy về VND — con số duy nhất cộng được giữa các đơn vị tiền.</summary>
    public decimal TotalVndConverted { get; set; }

    /// <summary>Phần USDT / USD đã quy về VND — API tính sẵn, UI không tự nhân tỷ giá.</summary>
    public decimal TotalUsdtInVnd { get; set; }
    public decimal TotalUsdInVnd { get; set; }

    /// <summary>Tổng cột "Mỗi brand" — tiền của bộ lọc sau khi đã chia đều theo số brand mỗi phiếu.</summary>
    public decimal TotalVndPerBrand { get; set; }

    /// <summary>Số phiếu không có brand nên không chia được, không nằm trong <see cref="TotalVndPerBrand"/>.</summary>
    public int NoBrandTickets { get; set; }
}

public class BoktIdentityRiskDto
{
    public string IdentityType { get; set; } = string.Empty;
    public string IdentityValue { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public int SupplierCount { get; set; }
    public int CreatorCount { get; set; }
    public int PicCount { get; set; }
    public DateTime? FirstDate { get; set; }
    public DateTime? LastDate { get; set; }
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public decimal TotalUsd { get; set; }
    public int RiskScore { get; set; }
    public string? RiskFlags { get; set; }
    public string? SampleTickets { get; set; }
}

public class BoktIdentityTicketDto
{
    public long RawId { get; set; }
    public string? TicketId { get; set; }
    public DateTime? NgayKt { get; set; }
    public string SheetName { get; set; } = string.Empty;
    public int SourceRow { get; set; }
    public string? Creator { get; set; }
    public string? Supplier { get; set; }
    public string? Pic { get; set; }
    public string IdentityType { get; set; } = string.Empty;
    public string IdentityValue { get; set; } = string.Empty;
    public string? NetworkHint { get; set; }
    public string? BankNameHint { get; set; }
    public string? AccountNameHint { get; set; }
    public string? ContextText { get; set; }
    public int ConfidenceScore { get; set; }
    public decimal? AmountValue { get; set; }
    public string? AmountCurrency { get; set; }
    public string ParseStatus { get; set; } = string.Empty;
}

public class BoktIdentityProfileDto
{
    public string IdentityType { get; set; } = string.Empty;
    public string IdentityValue { get; set; } = string.Empty;
    public string? BankNameHint { get; set; }
    public string? AccountNameHint { get; set; }
    public string? NetworkHint { get; set; }
    public int TotalTickets { get; set; }
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public decimal TotalUsd { get; set; }
    public decimal TotalUnknown { get; set; }
    public int PartialTickets { get; set; }
    public int MissingCurrencyTickets { get; set; }
    public DateTime? FirstDate { get; set; }
    public DateTime? LastDate { get; set; }
    public int DistinctSuppliers { get; set; }
    public int DistinctCreators { get; set; }
    public int DistinctPics { get; set; }
    public List<string> Suppliers { get; set; } = [];
    public List<string> Creators { get; set; } = [];
    public List<string> Pics { get; set; } = [];
    public int DistinctIdentities { get; set; }
    public List<string> Identities { get; set; } = [];
    public int RiskScore { get; set; }
    public List<string> RiskFlags { get; set; } = [];
    public List<BoktIdentityTimelinePoint> Timeline { get; set; } = [];
    public List<BoktIdentityBrandExposure> Brands { get; set; } = [];
    public List<BoktIdentityDayCluster> DayClusters { get; set; } = [];
}

public class BoktIdentityTimelinePoint
{
    public string Month { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public decimal TotalUsd { get; set; }
}

public class BoktIdentityBrandExposure
{
    public string Brand { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public decimal TotalUsd { get; set; }
}

public class BoktIdentityDayCluster
{
    public DateTime Date { get; set; }
    public int TicketCount { get; set; }
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public decimal TotalUsd { get; set; }
    public List<string> TicketIds { get; set; } = [];
}

public class BoktDomainQualityDto
{
    public string Domain { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public decimal AmountVnd { get; set; }
    public decimal AmountUsdt { get; set; }
    public decimal AmountUsd { get; set; }
    public decimal AmountUnknown { get; set; }
    public DateTime? AhrefsSnapshotDate { get; set; }
    public decimal? DomainRating { get; set; }
    public int? OrganicTraffic { get; set; }
    public int? OrganicKeywords { get; set; }
    public int? BacklinksTotal { get; set; }
    public int? ReferringDomains { get; set; }
    public bool HasAhrefs { get; set; }
}

public class BoktTicketAiAnalysisRequestDto
{
    public long RawId { get; set; }
}

public class BoktTicketAiAnalysisResultDto
{
    public decimal? SuggestedAmountValue { get; set; }
    public string? SuggestedAmountCurrency { get; set; }
    public string? SuggestedCostType { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}

public class BoktTicketApplyFixRequestDto
{
    public long RawId { get; set; }
    public decimal AmountValue { get; set; }
    public string AmountCurrency { get; set; } = string.Empty;
    public string? CostType { get; set; }
}

public class BoktBulkAiFixRequestDto : BoktCostCenterFilterDto
{
    public int MaxTickets { get; set; } = 100;

    /// <summary>When set, fix only these tickets instead of auto-selecting flagged rows from the filter.</summary>
    public List<long>? RawIds { get; set; }
}

public class BoktBulkAiFixResultDto
{
    public int Processed { get; set; }
    public int Fixed { get; set; }
    public int Failed { get; set; }
    public List<string> FailedTickets { get; set; } = [];
}
