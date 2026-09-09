using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Data.PicPerformance;

namespace BootstrapBlazor.Server.Data.SeoRequests;

public class SeoRequestBrandCodeDto
{
    public long Id { get; set; }
    public int BrandCodeId { get; set; }
    public string? BrandCodeName { get; set; }
}

public class SeoRequestStatusUpdateResultDto
{
    public long Id { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PostPurchaseStatus { get; set; }
}

public class SeoRequestDto
{
    public long Id { get; set; }
    public int? Stt { get; set; }
    public string? RequestCode { get; set; }
    public int? PicId { get; set; }
    public string? PicName { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public DateTime? RequestTime { get; set; }
    public int? CostTypeId { get; set; }
    public string? CostTypeName { get; set; }
    public string? Content { get; set; }
    public string? Domain { get; set; }
    public long? KeywordId { get; set; }
    public string? KeywordText { get; set; }
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }
    public int? BrandCodeId { get; set; }
    public string? BrandCodeName { get; set; }
    public int? Volume { get; set; }
    public int? DifficultyId { get; set; }
    public string? DifficultyName { get; set; }
    public string StatusKey { get; set; } = "SEO";
    public string PaymentStatus { get; set; } = "CHO_DUYET_TRIEN_KHAI";
    public string LeadStatus { get; set; } = "CHO_CHECK";
    public string IcStatus { get; set; } = "CHO_CHECK";
    public string HeadStatus { get; set; } = "CHO_DUYET";
    public string AcceptanceStatus { get; set; } = "CHO_CHECK";
    public string? FileBaogia { get; set; }
    public string? PartnerPaymentInfo { get; set; }

    // Structured partner payment — only the chosen method's fields are filled.
    public string? PartnerContact { get; set; }
    public string? PartnerPaymentMethod { get; set; }
    public string? PartnerBankAccountName { get; set; }
    public string? PartnerBankName { get; set; }
    public string? PartnerBankNumber { get; set; }
    public string? PartnerCryptoNetwork { get; set; }
    public string? PartnerCryptoAddress { get; set; }
    public string? FileOrder { get; set; }
    public int? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? Vnd { get; set; }
    public decimal? Usdt { get; set; }

    /// <summary>
    /// Cost in VND whatever currency was actually paid. Computed by the API so the exchange
    /// rate stays written down in one place. A ticket is paid in VND xor USDT and almost every
    /// one is USDT, which is why the raw <see cref="Vnd"/> reads 0 on nearly every row.
    /// </summary>
    public decimal VndEquivalent { get; set; }
    public string? LeadNote { get; set; }
    public string? PicNote { get; set; }
    public string? IcNote { get; set; }
    public string? HeadNote { get; set; }
    public DateTime? TicketDate { get; set; }
    public string? TicketNumber { get; set; }
    /// <summary>Internal payment ticket (n_acct_ticket.id) this row was gathered into.</summary>
    public long? AcctTicketId { get; set; }
    public string? AcctTicketErp { get; set; }
    public int? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string RequestType { get; set; } = "SEO_RESOURCE";
    public int? PicPositionId { get; set; }
    public string? PicPositionName { get; set; }
    public decimal? DomainPrice { get; set; }
    public string? DomainClassification { get; set; }
    public string? Market { get; set; }
    public string? DomainAge { get; set; }
    public decimal? DomainDr { get; set; }
    public decimal? DomainBl { get; set; }
    public decimal? DomainRd { get; set; }
    public string? UsagePurpose { get; set; }
    public string? PlanLink { get; set; }
    public string? Reason { get; set; }
    public string? FinalCheckStatus { get; set; }
    public string? PostPurchaseStatus { get; set; }
    public List<SeoRequestBrandCodeDto> BrandCodes { get; set; } = new();
}

public class SeoRequestFilterDto : BaseFilterPagingDto
{
    public string? RequestCode { get; set; }
    public string? Domain { get; set; }
    public int? PicId { get; set; }
    public int? TeamId { get; set; }
    public string? RequestType { get; set; }
    public int? CostTypeId { get; set; }
    public string? StatusKey { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? RequestTimeFrom { get; set; }
    public DateTime? RequestTimeTo { get; set; }
    public DateTime? TicketDateFrom { get; set; }
    public DateTime? TicketDateTo { get; set; }
    public string? Keyword { get; set; }
    public long? KeywordId { get; set; }
    public string? LeadStatus { get; set; }
    public string? IcStatus { get; set; }
    public string? AssistantStatus { get; set; }
    public string? HeadStatus { get; set; }
    public string? PostPurchaseStatus { get; set; }
    public string? PendingPipelineStep { get; set; }
    public string? PaymentPipelineStatus { get; set; }

    /// <summary>Approval queue step: LEAD | IC | ASSISTANT | HEAD | MINE.</summary>
    public string? ApprovalStep { get; set; }

    /// <summary>PENDING | APPROVED | REJECTED — ignored unless ApprovalStep is set.</summary>
    public string? ApprovalOutcome { get; set; }

    /// <summary>
    /// Column the API orders the page by: Id | RequestTime | Domain | Vnd | PaymentStatus.
    /// Sorting is server-side because the list is paged — the client only ever holds one page.
    /// </summary>
    public string? SortName { get; set; }

    /// <summary>Descending when true.</summary>
    public bool SortDesc { get; set; } = true;
}

public class SeoRequestDashboardDto
{
    public int TotalRequests { get; set; }
    public int LeadApprovedCount { get; set; }
    public int IcCheckCount { get; set; }
    public int HeadApprovedCount { get; set; }
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public decimal ResourceTotalVnd { get; set; }
    public decimal DomainTotalVnd { get; set; }
    public int PipelinePicCreated { get; set; }
    public int PipelineLeadApproved { get; set; }
    public int PipelineIcCheck { get; set; }
    public int PipelineAssistantCheck { get; set; }
    public int PipelineHeadApproved { get; set; }
    public int PipelinePaid { get; set; }
    public int PipelineChoTrienKhai { get; set; }
    public int PipelineImplementing { get; set; }
    public int PipelineDaTrienKhai { get; set; }
    public int PipelineKhongTrienKhai { get; set; }
    /// <summary>Drafts and tickets sent back for rework.</summary>
    public int PipelineDraft { get; set; }
    public int PipelineCompleted { get; set; }
    public List<SeoRequestApprovalStepCountDto> ApprovalStepCounts { get; set; } = new();
    public List<SeoRequestStatusKeyCountDto> StatusKeyBreakdown { get; set; } = new();
}

/// <summary>One approval step split into the three queues shown in the role-scoped panel.</summary>
public class SeoRequestApprovalStepCountDto
{
    public string Step { get; set; } = string.Empty;
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}

public class SeoRequestStatusKeyCountDto
{
    public string StatusKey { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class CreateSeoRequestBrandCodeDto
{
    public int BrandCodeId { get; set; }
}

public class CreateSeoRequestDto
{
    public string? RequestType { get; set; } = "SEO_RESOURCE";
    public int? PicId { get; set; }
    public int? TeamId { get; set; }
    public DateTime? RequestTime { get; set; }
    public int? CostTypeId { get; set; }
    public string? Content { get; set; }
    public string? Domain { get; set; }
    public long? KeywordId { get; set; }
    public int? BrandId { get; set; }
    public int? BrandCodeId { get; set; }
    public int? Volume { get; set; }
    public int? DifficultyId { get; set; }
    public string? StatusKey { get; set; } = "SEO";
    public string? PaymentStatus { get; set; }
    public string? FileBaogia { get; set; }
    public string? PartnerPaymentInfo { get; set; }

    // Structured partner payment — only the chosen method's fields are filled.
    public string? PartnerContact { get; set; }
    public string? PartnerPaymentMethod { get; set; }
    public string? PartnerBankAccountName { get; set; }
    public string? PartnerBankName { get; set; }
    public string? PartnerBankNumber { get; set; }
    public string? PartnerCryptoNetwork { get; set; }
    public string? PartnerCryptoAddress { get; set; }
    public string? FileOrder { get; set; }
    public int? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? Vnd { get; set; }
    public decimal? Usdt { get; set; }
    public string? LeadNote { get; set; }
    public string? PicNote { get; set; }
    public string? IcNote { get; set; }
    public string? HeadNote { get; set; }
    public DateTime? TicketDate { get; set; }
    public string? TicketNumber { get; set; }
    public int? CreatedById { get; set; }
    public bool IsDraft { get; set; }
    public int? PicPositionId { get; set; }
    public decimal? DomainPrice { get; set; }
    public string? DomainClassification { get; set; }
    public string? Market { get; set; }
    public string? DomainAge { get; set; }
    public decimal? DomainDr { get; set; }
    public decimal? DomainBl { get; set; }
    public decimal? DomainRd { get; set; }
    public string? UsagePurpose { get; set; }
    public string? PlanLink { get; set; }
    public string? Reason { get; set; }
    public List<CreateSeoRequestBrandCodeDto> BrandCodes { get; set; } = new();
}

public class UpdateSeoRequestDto : CreateSeoRequestDto
{
    public long Id { get; set; }
    public string? PostPurchaseStatus { get; set; }
}

public class UpdateSeoPostPurchaseStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateSeoPaymentStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class SeoDifficultyDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
}

public class SeoDifficultyFilterDto : BaseFilterPagingDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
}

public class SeoKeyStatusDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int Odx { get; set; }
    public bool IsActive { get; set; }
}

public class SeoKeyStatusFilterDto : BaseFilterPagingDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public static class SeoRequestTypes
{
    public const string SeoResource = "SEO_RESOURCE";
    public const string DomainPurchase = "DOMAIN_PURCHASE";
    /// <summary>UI-only tab on /seo-request — not a DB request_type value.</summary>
    public const string Inventory = "INVENTORY";
}

public static class SeoDomainClassifications
{
    public const string New = "New";
    public const string Aged = "aged";
}

public static class SeoMarkets
{
    public const string Vn = "VN";
    public const string Th = "TH";
}

public static class SeoPostPurchaseStatuses
{
    public const string ChoDuyet = "CHO_DUYET";
    public const string Duyet = "DUYET";
    public const string CannotPurchase = "CANNOT_PURCHASE";
    public const string AlreadyPurchased = "ALREADY_PURCHASED";
    public const string Done = "DONE";

    public static List<SelectedItem<string>> BuildOptions() =>
    [
        new SelectedItem<string>(ChoDuyet, "Chờ duyệt"),
        new SelectedItem<string>(Duyet, "Duyệt"),
        new SelectedItem<string>(CannotPurchase, "Không mua được"),
        new SelectedItem<string>(AlreadyPurchased, "Đã bị mua"),
        new SelectedItem<string>(Done, "Hoàn thành")
    ];

    public static string GetLabel(string? status) => status switch
    {
        ChoDuyet => "Chờ duyệt",
        Duyet => "Duyệt",
        CannotPurchase => "Không mua được",
        AlreadyPurchased => "Đã bị mua",
        Done => "Hoàn thành",
        "CANCEL" => "Hủy",
        _ => status ?? "-"
    };

    public static bool IsValid(string? status) =>
        status is ChoDuyet or Duyet or CannotPurchase or AlreadyPurchased or Done;
}

public class SeoRequestApprovalDto
{
    public long Id { get; set; }
    public string Step { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? Note { get; set; }
    public int? ActorId { get; set; }
    public DateTime? TicketDate { get; set; }
    public string? TicketNumber { get; set; }
    public string? PartnerPaymentInfo { get; set; }
}

public class SeoRequestLogDto
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public int? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SeoRequestDetailDto : SeoRequestDto
{
    public string AssistantStatus { get; set; } = "CHO_DUYET";
    public string? AssistantNote { get; set; }
    public string? LeadReviewerName { get; set; }
    public string? IcCheckerName { get; set; }
    public string? HeadApproverName { get; set; }
    public DateTime? LeadReviewedAt { get; set; }
    public DateTime? IcCheckedAt { get; set; }
    public DateTime? HeadApprovedAt { get; set; }
    public List<SeoRequestLogDto> Logs { get; set; } = new();
    public List<string> BrandCodeNames { get; set; } = new();
}

public static class SeoRequestApprovalSteps
{
    public const string Lead = "LEAD";
    public const string Ic = "IC";
    public const string Assistant = "ASSISTANT";
    public const string Head = "HEAD";

    /// <summary>Creator queue for SEO PIC accounts — their own tickets, not an approval step.</summary>
    public const string Mine = "MINE";
}

/// <summary>Which queue of an approval step to show or list.</summary>
public static class SeoRequestApprovalOutcomes
{
    public const string Pending = "PENDING";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
}

public static class SeoRequestApprovalActions
{
    public const string Approve = "APPROVE";
    public const string Reject = "REJECT";
    public const string RequestEdit = "REQUEST_EDIT";
}

public class SeoRequestMigrateRequestDto
{
    public string RequestType { get; set; } = "SEO_RESOURCE";
    public int? DefaultTeamId { get; set; }
    public bool DryRun { get; set; } = true;
    public List<string> Headers { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
    public string? SourceLabel { get; set; }
}

public class SeoRequestMigrateResultDto
{
    public bool Success { get; set; }
    public bool DryRun { get; set; }
    public int Inserted { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public int GeneratedCodes { get; set; }
    public List<string> Messages { get; set; } = new();
}

public class SeoRequestPicKeywordSummaryRequestDto
{
    public int? PicId { get; set; }
    public long? KeywordId { get; set; }
    public long? ExcludeRequestId { get; set; }
}

public class SeoRequestPicKeywordSummaryDto
{
    public int? PicId { get; set; }
    public long? KeywordId { get; set; }
    public string? PicName { get; set; }
    public string? KeywordText { get; set; }
    public int RequestCount { get; set; }
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public List<SeoRequestPicKeywordMonthDto> Months { get; set; } = new();
}

public class SeoRequestPicKeywordMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

/// <summary>
/// Chuẩn mà một phiếu phải đối chiếu: thứ hạng phải đạt ở tháng thứ N và trần chi phí của độ khó đó.
/// Nguồn: bảng hệ quy chiếu trong DB (trước đây là Google Sheet).
/// </summary>
public class SeoKpiReferenceDto
{
    public int DifficultyId { get; set; }
    public string? DifficultyName { get; set; }
    public int MonthIndex { get; set; }
    public string? StandardLabel { get; set; }
    public int? ExpectedMin { get; set; }
    public int? ExpectedMax { get; set; }
    public bool RequiresIndex { get; set; }
    public bool IsMaintain { get; set; }
    public bool HasStandard { get; set; }
    public decimal? BudgetCapVnd { get; set; }
    public int? BudgetMonthsCovered { get; set; }
    public int? MaxSitesPerKeyword { get; set; }
    public bool BudgetCapIsFloor { get; set; }
}

/// <summary>Ngân sách của key mà phiếu thuộc về: đã chi, tháng thứ mấy, trần, còn hay vượt.</summary>
public class SeoRequestBudgetDto
{
    public long RequestId { get; set; }
    public long? KeywordId { get; set; }
    public string? KeywordText { get; set; }
    public string? DifficultyName { get; set; }
    public bool PicScoped { get; set; }
    public DateTime? FirstCostDate { get; set; }
    public int? MonthIndex { get; set; }
    public decimal SpentVnd { get; set; }
    public int CountedTickets { get; set; }
    public int ExcludedTickets { get; set; }
    public decimal? CapVnd { get; set; }
    public int CapMonths { get; set; }
    public bool CapApplies { get; set; }
    public decimal? RemainingVnd { get; set; }
    public bool IsOverCap { get; set; }
    public string? StandardLabel { get; set; }
    public int? StandardExpectedMin { get; set; }
    public int? StandardExpectedMax { get; set; }
    public bool StandardIsMaintain { get; set; }
    public List<SeoRequestDomainSpendDto> DomainSpends { get; set; } = new();
}

public class SeoRequestDomainSpendDto
{
    public string Domain { get; set; } = string.Empty;
    public decimal SpentVnd { get; set; }
    public int TicketCount { get; set; }
}

/// <summary>Một mốc thứ hạng kèm nguồn — người duyệt phải biết con số ở đâu ra.</summary>
public class SeoRequestRankMarkDto
{
    /// <summary>"no-data" | "outside-top-10" | "found". Ba trạng thái khác nhau, không gộp.</summary>
    public string State { get; set; } = "no-data";
    public int? Position { get; set; }
    public DateTime? AtDate { get; set; }

    /// <summary>"feed" = bảng theo dõi hằng ngày; "scan" = có người bấm quét.</summary>
    public string Source { get; set; } = "feed";
    public int? TrackedTopN { get; set; }

    /// <summary>Mốc gốc tra ngược lịch sử, không phải chụp lúc tạo phiếu.</summary>
    public bool IsRetroactive { get; set; }

    public bool IsFound => State == "found" && Position.HasValue;
    public bool IsOutside => State == "outside-top-10";
}

public class SeoRequestRankSnapshotDto
{
    public long Id { get; set; }
    public DateTime ScannedAt { get; set; }
    public string Source { get; set; } = "scan";
    public bool? IsIndexed { get; set; }
    public int? IndexedCount { get; set; }
    public int? RankPosition { get; set; }
    public int? ScannedTopN { get; set; }
    public string? Note { get; set; }
}

public class SaveSeoRequestRankScanDto
{
    public long RequestId { get; set; }
    public string? KeywordText { get; set; }
    public string? Domain { get; set; }
    public bool? IsIndexed { get; set; }
    public int? IndexedCount { get; set; }
    public int? RankPosition { get; set; }
    public int? ScannedTopN { get; set; }
    public string? Note { get; set; }
}

public class SeoRequestRankHistoryDto
{
    public SeoRequestRankSnapshotDto? First { get; set; }
    public SeoRequestRankSnapshotDto? Latest { get; set; }
    public int TotalScans { get; set; }
    public int? RankDelta { get; set; }
    public int? DaysBetween { get; set; }
    public SeoRequestRankMarkDto? Baseline { get; set; }
    public SeoRequestRankMarkDto? Current { get; set; }

    /// <summary>Keyword của phiếu có nằm trong bảng theo dõi hằng ngày không.</summary>
    public bool KeywordTracked { get; set; }
}

/// <summary>
/// Kết luận của hệ thống, chốt lúc nộp phiếu. Khác với các thẻ bên dưới — chúng hiện số HIỆN TẠI.
/// Người duyệt phải thấy được cả hai: cái đúng lúc duyệt, và cái đúng hôm nay.
/// </summary>
public class SeoRequestSystemVerdictDto
{
    public long RequestId { get; set; }
    public DateTime ComputedAt { get; set; }

    /// <summary>"pass" | "review" | "block". IC luôn phải bấm, kể cả "pass".</summary>
    public string Verdict { get; set; } = "review";

    public int? MonthIndex { get; set; }
    public int? RankPosition { get; set; }
    public string? RankState { get; set; }
    public int? RankDelta30d { get; set; }
    /// <summary>"pubseo" | "redirect" | "pbn" | "other" | "unknown" — chỉ pubseo mới chấm theo thứ hạng.</summary>
    public string? SiteType { get; set; }
    public string? DomainState { get; set; }
    public bool? IsTopDomainOfKey { get; set; }
    public int? TeamCountOnKey { get; set; }
    public decimal? KeySpendAllTeams { get; set; }
    public decimal? TeamSpendOnKey { get; set; }
    public decimal? SpentVnd { get; set; }
    public decimal? CapVnd { get; set; }
    public bool? EverInTop10 { get; set; }

    /// <summary>Mỗi dòng một câu, để IC đọc rồi bấm chứ không phải gõ lại.</summary>
    public List<string> Reasons { get; set; } = [];

    public bool IsPass => Verdict == "pass";
    public bool IsBlock => Verdict == "block";
}

/// <summary>Cả bảng chuẩn của một mức độ khó — popup người duyệt mở ra xem.</summary>
public class SeoKpiStandardTableDto
{
    public int DifficultyId { get; set; }
    public string? DifficultyName { get; set; }
    public decimal? CapVnd { get; set; }
    public int? CapMonths { get; set; }
    public int? MaxSites { get; set; }
    public bool CapIsFloor { get; set; }
    public List<SeoKpiStandardRowDto> Rows { get; set; } = [];
}

public class SeoKpiStandardRowDto
{
    public int MonthIndex { get; set; }
    public string? Label { get; set; }
    public int? ExpectedMin { get; set; }
    public int? ExpectedMax { get; set; }
    public bool RequiresIndex { get; set; }
    public bool IsMaintain { get; set; }
}

/// <summary>Payout details of a partner from the n_supplier catalogue, mirrored from the API.</summary>
public class SeoRequestPartnerPaymentDto
{
    public string? PartnerContact { get; set; }
    public string? PartnerPaymentMethod { get; set; }
    public string? PartnerBankAccountName { get; set; }
    public string? PartnerBankName { get; set; }
    public string? PartnerBankNumber { get; set; }
    public string? PartnerCryptoNetwork { get; set; }
    public string? PartnerCryptoAddress { get; set; }
}

public class CreateAcctTicketDto
{
    public List<long> Ids { get; set; } = new();
    /// <summary>True once the IC has read the duplicate warning and still wants the ticket.</summary>
    public bool ConfirmDuplicate { get; set; }
}

public class AcctTicketCreatedDto
{
    public long Id { get; set; }
    public int Count { get; set; }
    public List<AcctTicketDuplicateDto> Duplicates { get; set; } = new();
}

/// <summary>A picked row repeating an order already inside another ticket.</summary>
public class AcctTicketDuplicateDto
{
    public long RequestId { get; set; }
    public long DuplicateOfRequestId { get; set; }
    public long TicketId { get; set; }
    public string? ErpTicketNumber { get; set; }
    public string? Domain { get; set; }
    public decimal Vnd { get; set; }
    public decimal Usdt { get; set; }
}

public class AcctTicketErpDto
{
    public string ErpTicketNumber { get; set; } = string.Empty;
}

public class AcctTicketListItemDto
{
    public long Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErpTicketNumber { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public decimal ExchangeRate { get; set; }
    /// <summary>Paid amount in VND after converting USDT at the ticket's rate.</summary>
    public decimal PaidVnd { get; set; }
    public string? PicNames { get; set; }
    public string? PartnerContacts { get; set; }
    public bool HasPayoutTarget { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AcctTicketDetailDto
{
    public long Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErpTicketNumber { get; set; }
    public string? Note { get; set; }
    public string? Content { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal ExchangeRate { get; set; }
    public SeoRequestPartnerPaymentDto? Payout { get; set; }
    public string? SupplierCode { get; set; }
    public List<SeoRequestDto> Items { get; set; } = new();
}

public class AcctTicketUpdateDto
{
    public string? Note { get; set; }
    public string? Content { get; set; }
    public decimal? ExchangeRate { get; set; }
}

/// <summary>The four decision blocks in one payload; FromSnapshot = stored numbers of an approved/paid request.</summary>
public class SeoRequestMetricsDto
{
    public SeoRequestBudgetDto? Budget { get; set; }
    public PicPerformancePicDetailDto? Pic { get; set; }
    public SeoRequestRankHistoryDto? Rank { get; set; }
    public SeoRequestSystemVerdictDto? Verdict { get; set; }
    public bool FromSnapshot { get; set; }
    public DateTime? SnapshotAt { get; set; }
}
