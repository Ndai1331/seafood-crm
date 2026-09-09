using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace BootstrapBlazor.Server.Data
{
  
    public class SeoPaymentTicketTotalDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("reportDate")]
        public DateTime ReportDate { get; set; }

        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("ticketId")]
        public string? TicketId { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("currencyId")]
        public int CurrencyId { get; set; }

        [JsonPropertyName("currencyCode")]
        public string? CurrencyCode { get; set; }

        [JsonPropertyName("currencyName")]
        public string? CurrencyName { get; set; }

        [JsonPropertyName("exchangeRate")]
        public double ExchangeRate { get; set; }

        [JsonPropertyName("totalAmount")]
        public double TotalAmount { get; set; }

        [JsonPropertyName("totalAmountVnd")]
        public double TotalAmountVnd { get; set; }

        [JsonPropertyName("createdById")]
        public int CreatedById { get; set; }

        [JsonPropertyName("createdByName")]
        public string? CreatedByName { get; set; }

        [JsonPropertyName("picId")]
        public int? PicId { get; set; }

        [JsonPropertyName("picName")]
        public string? PicName { get; set; }

        [JsonPropertyName("seoPaymentTickets")]
        public List<SeoPaymentTicketDetailDto>? SeoPaymentTickets { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("approvalStep")]
        public int ApprovalStep { get; set; }

        [JsonPropertyName("approvalStepName")]
        public string? ApprovalStepName { get; set; }

        [JsonPropertyName("approvalStatus")]
        public int ApprovalStatus { get; set; }

        [JsonPropertyName("approvalStatusName")]
        public string? ApprovalStatusName { get; set; }

        [JsonPropertyName("icCheckedById")]
        
        public int? IcCheckedById { get; set; }

        [JsonPropertyName("icCheckedByName")]
        public string? IcCheckedByName { get; set; }

        [JsonPropertyName("icCheckedAt")]
        public DateTime? IcCheckedAt { get; set; }
        
        [JsonPropertyName("assistantReviewedById")]
        public int? AssistantReviewedById { get; set; }

        [JsonPropertyName("assistantReviewedByName")]
        public string? AssistantReviewedByName { get; set; }

        [JsonPropertyName("assistantReviewedAt")]
        public DateTime? AssistantReviewedAt { get; set; }
        
        [JsonPropertyName("approvedById")]
        public int? ApprovedById { get; set; }

        [JsonPropertyName("approvedByName")]
        public string? ApprovedByName { get; set; }

        [JsonPropertyName("approvedAt")]
        public DateTime? ApprovedAt { get; set; }
        
        [JsonPropertyName("rejectedById")]
        public int? RejectedById { get; set; }

        [JsonPropertyName("rejectedByName")]
        public string? RejectedByName { get; set; }

        [JsonPropertyName("rejectedAt")]
        public DateTime? RejectedAt { get; set; }

        [JsonPropertyName("rejectReason")]
        public string? RejectReason { get; set; } = string.Empty;


        [JsonPropertyName("approveReason")]
        public string? ApproveReason { get; set; } = string.Empty;

        [JsonPropertyName("rejectStep")]
        public int? RejectStep { get; set; }

        [JsonPropertyName("rejectStepName")]
        public string? RejectStepName { get; set; }

        [JsonPropertyName("approveStep")]
        public int? ApproveStep { get; set; }

        [JsonPropertyName("approveStepName")]
        public string? ApproveStepName { get; set; }


        [JsonPropertyName("icCheckComment")]
        public string? IcCheckComment { get; set; }

        [JsonPropertyName("assistantReviewComment")]
        public string? AssistantReviewComment { get; set; }

        [JsonPropertyName("managerApproveComment")]
        public string? ManagerApproveComment { get; set; }

        [JsonPropertyName("resubmitComment")]
        public string? ResubmitComment { get; set; }

        
    }

    /// <summary>
    /// SEO Payment Ticket Detail DTO
    /// </summary>
    public class SeoPaymentTicketDetailDto
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// SEO Payment Ticket Total ID.
        /// </summary>
        public long SeoPaymentTicketTotalId { get; set; }

        /// <summary>
        /// Bank account number (optional).
        /// </summary>
        public string? BankAccountNumber { get; set; }

        /// <summary>
        /// Bank account name (optional).
        /// </summary>
        public string? BankAccountName { get; set; }

        /// <summary>
        /// Bank name (optional).
        /// </summary>
        public string? BankName { get; set; }

        /// <summary>
        /// Payment ticket description/content (optional).
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Cost type ID (required).
        /// </summary>
        public int CostTypeId { get; set; }

        /// <summary>
        /// Cost type name (from API response).
        /// </summary>
        public string? CostTypeName { get; set; }

        /// <summary>
        /// Quantity (optional).
        /// </summary>
        public double? Quantity { get; set; }

        /// <summary>
        /// Domain name (optional).
        /// </summary>
        public string? Domain { get; set; }
        /// <summary>
        /// List of domains (optional).
        /// </summary>
        public List<string>? Domains { get; set; }
        /// <summary>
        /// Dictionary of domains and their corresponding prices extracted from content (optional).
        /// </summary>
        public Dictionary<string, decimal>? DomainsPrices { get; set; }

        /// <summary>
        /// Total amount (required).
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// Total amount in VND (from API response).
        /// </summary>
        public double TotalAmountVnd { get; set; }

        /// <summary>
        /// Report link (optional).
        /// </summary>
        public string? ReportLink { get; set; }

        /// <summary>
        /// Person in charge ID (optional).
        /// </summary>
        public int? PicId { get; set; }

        /// <summary>
        /// Person in charge name (from API response).
        /// </summary>
        public string? PicName { get; set; }

        /// <summary>
        /// List of brand codes associated with this payment ticket.
        /// </summary>
        public List<SeoTicketBrandCodeDto>? BrandCodes { get; set; }
        public List<SeoTicketKeywordDto>? Keywords { get; set; }
    }

    public class SeoTicketKeywordDto
    {
        public long Id { get; set; }
        public long SeoPaymentTicketId { get; set; }
        public long KeywordId { get; set; }
        public string? KeywordText { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountVnd { get; set; }
    }

    /// <summary>
    /// Brand Code DTO for SEO Payment Tickets
    /// </summary>
    public class SeoTicketBrandCodeDto
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// SEO Payment Ticket ID.
        /// </summary>
        public long SeoPaymentTicketId { get; set; }

        /// <summary>
        /// Brand Code ID (required).
        /// </summary>
        public int BrandCodeId { get; set; }

        /// <summary>
        /// Brand Code name (from API response).
        /// </summary>
        public string? BrandCodeName { get; set; }

        /// <summary>
        /// Amount.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Amount in VND.
        /// </summary>
        public double AmountVnd { get; set; }
    }

    /// <summary>
    /// DTO for creating a new SEO Payment Ticket Total - matches API specification
    /// </summary>
    public class CreateSeoPaymentTicketTotalDto
    {
        /// <summary>
        /// Report date (required).
        /// </summary>
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// Month (1-12, required).
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Ticket ID (optional, max 50 chars).
        /// </summary>
        public string? TicketId { get; set; }

        /// <summary>
        /// Note/description (optional).
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Currency ID (required).
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        /// Exchange rate (required, min 0.0001).
        /// </summary>
        public double ExchangeRate { get; set; }

        /// <summary>
        /// Created by ID (required).
        /// </summary>
        public int CreatedById { get; set; }

        /// <summary>
        /// Person in charge ID (optional).
        /// </summary>
        public int? PicId { get; set; }

        /// <summary>
        /// List of SEO payment tickets (required, at least 1).
        /// </summary>
        public List<CreateSeoPaymentTicketDetailDto> SeoPaymentTickets { get; set; } = new List<CreateSeoPaymentTicketDetailDto>();
    }

    /// <summary>
    /// DTO for creating SEO Payment Ticket Details
    /// </summary>
    public class CreateSeoPaymentTicketDetailDto
    {
        /// <summary>
        /// Bank account number (optional, max 50 chars).
        /// </summary>
        public string? BankAccountNumber { get; set; }

        /// <summary>
        /// Bank account name (optional, max 150 chars).
        /// </summary>
        public string? BankAccountName { get; set; }

        /// <summary>
        /// Bank name (optional, max 80 chars).
        /// </summary>
        public string? BankName { get; set; }

        /// <summary>
        /// Payment ticket description/content (optional).
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Cost type ID (required).
        /// </summary>
        public int CostTypeId { get; set; }

        /// <summary>
        /// Quantity (optional).
        /// </summary>
        public double? Quantity { get; set; }

        /// <summary>
        /// Domain name (optional, max 150 chars).
        /// </summary>
        public string? Domain { get; set; }
        public List<string>? Domains { get; set; } // List of domains

        public Dictionary<string, decimal>? DomainsPrices { get; set; } // Dictionary of domains and prices

        /// <summary>
        /// Total amount (required, min 0).
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// Report link (optional, max 500 chars).
        /// </summary>
        public string? ReportLink { get; set; }

        /// <summary>
        /// Person in charge ID (optional).
        /// </summary>
        public int? PicId { get; set; }

        /// <summary>
        /// List of brand codes associated with this payment ticket (required, at least 1).
        /// </summary>
        public List<CreateSeoTicketBrandCodeDto> BrandCodes { get; set; } = new List<CreateSeoTicketBrandCodeDto>();

        /// <summary>
        /// List of keywords associated with this payment ticket (optional).
        /// </summary>
        public List<CreateSeoTicketKeywordDto> Keywords { get; set; } = new List<CreateSeoTicketKeywordDto>();
    }

    /// <summary>
    /// DTO for creating brand codes for SEO Payment Tickets
    /// </summary>
    public class CreateSeoTicketBrandCodeDto
    {
        /// <summary>
        /// Brand Code ID (required).
        /// </summary>
        public int BrandCodeId { get; set; }
    }

    /// <summary>
    /// DTO for creating keywords for SEO Payment Tickets
    /// </summary>
    public class CreateSeoTicketKeywordDto
    {
        /// <summary>
        /// Keyword text (required).
        /// </summary>
        public string KeywordText { get; set; } = string.Empty;
        public long KeywordId { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing SEO Payment Ticket Total - matches API specification
    /// </summary>
    public class UpdateSeoPaymentTicketTotalDto
    {
        /// <summary>
        /// Unique identifier of the payment ticket total to update (required).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Report date (required).
        /// </summary>
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// Month (1-12, required).
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Ticket ID (optional, max 50 chars).
        /// </summary>
        public string? TicketId { get; set; }

        /// <summary>
        /// Note/description (optional).
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Currency ID (required).
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        /// Exchange rate (required, min 0.0001).
        /// </summary>
        public double ExchangeRate { get; set; }

        /// <summary>
        /// Person in charge ID (optional).
        /// </summary>
        public int? PicId { get; set; }

        /// <summary>
        /// List of SEO payment tickets (required, at least 1).
        /// </summary>
        public List<UpdateSeoPaymentTicketDetailDto> SeoPaymentTickets { get; set; } = new List<UpdateSeoPaymentTicketDetailDto>();
    }

    /// <summary>
    /// DTO for updating SEO Payment Ticket Details
    /// </summary>
    public class UpdateSeoPaymentTicketDetailDto
    {
        /// <summary>
        /// Unique identifier (optional for new details, required for existing).
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// Bank account number (optional, max 50 chars).
        /// </summary>
        public string? BankAccountNumber { get; set; }

        /// <summary>
        /// Bank account name (optional, max 150 chars).
        /// </summary>
        public string? BankAccountName { get; set; }

        /// <summary>
        /// Bank name (optional, max 80 chars).
        /// </summary>
        public string? BankName { get; set; }

        /// <summary>
        /// Payment ticket description/content (optional).
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Cost type ID (required).
        /// </summary>
        public int CostTypeId { get; set; }

        /// <summary>
        /// Quantity (optional).
        /// </summary>
        public double? Quantity { get; set; }

        /// <summary>
        /// Domain name (optional, max 150 chars).
        /// </summary>
        public string? Domain { get; set; }

        public List<string>? Domains { get; set; } // List of domains
        public Dictionary<string, decimal>? DomainsPrices { get; set; } // Dictionary of domains and prices


        /// <summary>
        /// Total amount (required, min 0).
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// Report link (optional, max 500 chars).
        /// </summary>
        public string? ReportLink { get; set; }

        /// <summary>
        /// Person in charge ID (optional).
        /// </summary>
        public int? PicId { get; set; }

        /// <summary>
        /// List of brand codes associated with this payment ticket (required, at least 1).
        /// </summary>
        public List<CreateSeoTicketBrandCodeDto> BrandCodes { get; set; } = new List<CreateSeoTicketBrandCodeDto>();

        /// <summary>
        /// List of keywords associated with this payment ticket (optional).
        /// </summary>
        public List<CreateSeoTicketKeywordDto> Keywords { get; set; } = new List<CreateSeoTicketKeywordDto>();
    }

    /// <summary>
    /// Filter DTO for SEO Payment Ticket Total list query
    /// </summary>
    public class SeoPaymentTicketTotalFilterDto : BaseFilterPagingDto
    {
        /// <summary>
        /// Filter by from date
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Filter by to date
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Filter by month (1-12)
        /// </summary>
        public byte? Month { get; set; }

        /// <summary>
        /// Filter by ticket ID
        /// </summary>
        public string? TicketId { get; set; }

        /// <summary>
        /// Filter by created by user ID
        /// </summary>
        public int? CreatedById { get; set; }

        /// <summary>
        /// Filter by domain
        /// </summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Filter by person in charge ID
        /// </summary>
        public int? PicId { get; set; } = null;

        /// <summary>
        /// Filter by brand ID
        /// </summary>
        public int? BrandId { get; set; } = null;

        /// <summary>
        /// Filter by cost type ID
        /// </summary>
        public int? CostTypeId { get; set; } = null;

        /// <summary>
        /// Filter by approval step
        /// </summary>
        public int? ApprovalStep { get; set; }
    }

    /// <summary>
    /// Dashboard DTO for SEO Payment Ticket statistics
    /// </summary>
    public class SeoPaymentTicketDashboardDto
    {
        /// <summary>
        /// Brand statistics list
        /// </summary>
        public List<DashboardBrandDto> BrandStatistics { get; set; } = new List<DashboardBrandDto>();

        /// <summary>
        /// Domain statistics list
        /// </summary>
        public List<DashboardDomainDto> DomainStatistics { get; set; } = new List<DashboardDomainDto>();
        public List<DashboardBrandMonthlyDto> BrandMonthlyStatistics { get; set; } = new List<DashboardBrandMonthlyDto>();
        public List<DashboardPicDto> PicStatistics { get; set; } = new List<DashboardPicDto>();

    }
    /// <summary>
    /// Dashboard Brand DTO
    /// </summary>
    public class DashboardBrandDto
    {
        /// <summary>
        /// Brand code ID
        /// </summary>
        public int BrandCodeId { get; set; }

        /// <summary>
        /// Brand code name
        /// </summary>
        public string BrandCodeName { get; set; } = string.Empty;

        /// <summary>
        /// Total amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Total amount in VND
        /// </summary>
        public decimal TotalAmountVnd { get; set; }

        /// <summary>
        /// Ticket count
        /// </summary>
        public int TicketCount { get; set; }
        public List<CostTypeBreakdownDto> CostTypeBreakdown { get; set; } = new List<CostTypeBreakdownDto>();

    }


     public class DashboardPicDto
    {
        public int? PicId { get; set; }
        public string PicName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountVnd { get; set; }
        public int TicketCount { get; set; }
        public int DomainCount { get; set; }
        public int BrandCount { get; set; }
        public List<CostTypeBreakdownDto> CostTypeBreakdown { get; set; } = new List<CostTypeBreakdownDto>();
    }

    /// <summary>
    /// Dashboard Domain DTO
    /// </summary>
    public class DashboardDomainDto
    {
        /// <summary>
        /// Domain name
        /// </summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Total amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Total amount in VND
        /// </summary>
        public decimal TotalAmountVnd { get; set; }

        /// <summary>
        /// Ticket count
        /// </summary>
        public int TicketCount { get; set; }

        /// <summary>
        /// Brand count
        /// </summary>
        public int BrandCount { get; set; }
        public List<CostTypeBreakdownDto> CostTypeBreakdown { get; set; } = new List<CostTypeBreakdownDto>();

    }


    public class DashboardBrandMonthlyDto
    {
        public int BrandCodeId { get; set; }
        public string BrandCodeName { get; set; } = string.Empty;
        public List<MonthlyDataPoint> MonthlyData { get; set; } = new List<MonthlyDataPoint>();
    }

    /// <summary>
    /// Monthly data point for line chart
    /// </summary>
    public class MonthlyDataPoint
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthYear { get; set; } = string.Empty; // Format: "MM/YYYY" or "YYYY-MM"
        public decimal Amount { get; set; }
        public decimal AmountVnd { get; set; }
        public int TicketCount { get; set; }
    }




    public class CostTypeBreakdownDto
    {
        public int CostTypeId { get; set; }
        public string CostTypeName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountVnd { get; set; }
        public int TicketCount { get; set; }
    }

    /// <summary>
    /// DTO for approving SEO payment ticket at any step
    /// </summary>
    public class ApproveSeoPaymentTicketTotalDto
    {
        /// <summary>
        /// Ticket ID (required)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Approved by user ID (required)
        /// </summary>
        public int ApprovedById { get; set; }

        /// <summary>
        /// Optional comment/note for approval
        /// </summary>
        public string? Comment { get; set; }
    }

    /// <summary>
    /// DTO for rejecting SEO payment ticket at any step
    /// </summary>
    public class RejectSeoPaymentTicketTotalDto
    {
        /// <summary>
        /// Ticket ID (required)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Rejected by user ID (required)
        /// </summary>
        public int RejectedById { get; set; }

        /// <summary>
        /// Reject reason (required, max 2000 characters)
        /// </summary>
        public string RejectReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for resubmitting SEO payment ticket after rejection
    /// </summary>
    public class ResubmitSeoPaymentTicketTotalDto
    {
        /// <summary>
        /// Ticket ID (required)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Optional note for resubmission
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Optional approval step to set when resubmitting (if null, uses default flow)
        /// </summary>
        public int? ApprovalStep { get; set; }
    }
}